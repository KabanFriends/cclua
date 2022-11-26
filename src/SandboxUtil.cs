using CCLua.LuaObjects;
using NLua;

namespace CCLua
{
    public static class SandboxUtil
    {
        public const string SANDBOX_ALLOWED_FUNCTIONS = @"
_VERSION assert error    ipairs   next pairs
pcall    select tonumber tostring type unpack xpcall

math.abs   math.acos math.asin  math.atan math.atan2 math.ceil
math.cos   math.cosh math.deg   math.exp  math.fmod  math.floor
math.frexp math.huge math.ldexp math.log  math.log10 math.max
math.min   math.modf math.pi    math.pow  math.rad   math.random
math.sin   math.sinh math.sqrt  math.tan  math.tanh

os.clock os.difftime os.time

string.byte string.char  string.find  string.format string.gmatch
string.gsub string.len   string.lower string.match  string.reverse
string.sub  string.upper

table.insert table.maxn table.remove table.sort
thread.create delay level
";

        public const string PREDEFINED_FUNCTIONS = @"
thread = {}
schedules = {}
players = {}

delay = function(ms)
    coroutine.yield(ms)
end

thread.internalCreate = function(f, player)
    local co = coroutine.create(f)
    local sch = {thread = co, player = player, sleepUntil = 0}
    table.insert(schedules, sch)
end

thread.create = function(f, p)
    n = nil
    if p ~= nil then
        n = p.name
    end
    local co = coroutine.create(f)
    local sch = {thread = co, player = n, sleepUntil = 0}
    table.insert(schedules, sch)
end

wrapnum = function(max, num)
    if num >= 0 then
        return num % max
    else
        return (num % max + max) % max
    end
end

clampnum = function(num, min, max)
    if num < min then return min end
    if num > max then return max end
    return num
end

level = context.caller:Call('CCLua.LevelUtil', 'GetLevelObject', context.lua)
";

        public const string SANDBOX_LUA_CODE = @"
sandbox = {
  _VERSION      = ""sandbox 0.5"",
  _DESCRIPTION  = ""A pure-lua solution for running untrusted Lua code."",
  _URL          = ""https://github.com/kikito/sandbox.lua"",
  _LICENSE      = [[
    MIT LICENSE

    Copyright (c) 2021 Enrique García Cota

    Permission is hereby granted, free of charge, to any person obtaining a
    copy of this software and associated documentation files (the
    ""Software""), to deal in the Software without restriction, including
    without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to
    permit persons to whom the Software is furnished to do so, subject to
    the following conditions:

    The above copyright notice and this permission notice shall be included
    in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS
    OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
    SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
  ]],

}

" + PREDEFINED_FUNCTIONS + @"

-- PUC-Rio Lua 5.1 does not support deactivation of bytecode
local bytecode_blocked = _ENV or type(_G.jit) == ""table""
sandbox.bytecode_blocked = bytecode_blocked

-- The base environment is merged with the given env option (or an empty table, if no env provided)
--
local BASE_ENV = {}

-- List of unsafe packages/functions:
--
-- * string.rep: can be used to allocate millions of bytes in 1 operation
-- * {set|get}metatable: can be used to modify the metatable of global objects (strings, integers)
-- * collectgarbage: can affect performance of other systems
-- * dofile: can access the server filesystem
-- * _G: It has access to everything. It can be mocked to other things though.
-- * load{file|string}: All unsafe because they can grant acces to global env
-- * raw{get|set|equal}: Potentially unsafe
-- * module|require|module: Can modify the host settings
-- * string.dump: Can display confidential server info (implementation of functions)
-- * math.randomseed: Can affect the host sytem
-- * io.*, os.*: Most stuff there is unsafe, see below for exceptions


-- Safe packages/functions below
([[

" + SANDBOX_ALLOWED_FUNCTIONS + @"

]]):gsub('%S+', function(id)
  local module, method = id:match('([^%.]+)%.([^%.]+)')
  if module then
    BASE_ENV[module]         = BASE_ENV[module] or {}
    BASE_ENV[module][method] = _G[module][method]
  else
    BASE_ENV[id] = _G[id]
  end
end)

local function protect_module(module, module_name)
  return setmetatable({}, {
    __index = module,
    __newindex = function(_, attr_name, _)
      error('Can not modify ' .. module_name .. '.' .. attr_name .. '. Protected by the sandbox.')
    end
  })
end

('coroutine math os string table'):gsub('%S+', function(module_name)
  BASE_ENV[module_name] = protect_module(BASE_ENV[module_name], module_name)
end)

-- auxiliary functions/variables

local string_rep = string.rep

local function cleanup()
  string.rep = string_rep -- luacheck: no global
end

-- Public interface: sandbox.protect
function sandbox.protect(code, options)
  options = options or {}

  assert(type(code) == 'string', ""expected a string"")

  local passed_env = options.env or {}
  env = {}
  for k, v in pairs(BASE_ENV) do
    local pv = passed_env[k]
    if pv ~= nil then
      env[k] = pv
    else
      env[k] = v
    end
  end
  setmetatable(env, { __index = options.env })
  env._G = env

  local f
  if bytecode_blocked then
    f = assert(load(code, options.source, 't', env))
  else
    f = assert(loadstring(code))
    setfenv(f, env)
  end

  return function(...)
    string.rep = nil -- luacheck: no global

    local t = table.pack(pcall(f, ...))

    cleanup()

    if not t[1] then error(t[2]) end

    return table.unpack(t, 2, t.n)
  end
end

-- Public interface: sandbox.run
function sandbox.run(code, options, ...)
  return sandbox.protect(code, options) (...)
end

-- make sandbox(f) == sandbox.protect(f)
setmetatable(sandbox, { __call = function(_, code, o) return sandbox.protect(code, o) end})

return sandbox
";

        public static void SetupEnvironment(LuaContext context)
        {
            context.lua.DebugHook += context.CheckExecution;
            context.lua.SetDebugHook(KeraLua.LuaHookMask.Count, context.config.instructionsPerExecutionCheck);

            context.lua["context"] = context;

            context.lua.DoString(SANDBOX_LUA_CODE);
        }

        public static void CallFunction(Lua lua, string function, params object[] args)
        {
            lua.GetFunction("env." + function).Call(args);
        }
    }
}
