﻿using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaPlayerClickEventSupplier : LuaObjectSupplier<PlayerClickEvent>
    {
        public LuaPlayerClickEventSupplier(PlayerClickEvent e) : base(e)
        {
        }

        public static LuaTable Supply(Lua lua, PlayerClickEvent e)
        {
            lua["csEvent"] = e;

            return (LuaTable)lua.DoString(@"
local e = {}
local meta = {}
setmetatable(e, meta)

meta.obj = csEvent
meta.__tostring = function()
    return 'Event'
end

e.player = context.caller:Call('CCLua.LuaObjects.Suppliers.LuaPlayerSupplier', 'Supply', context.lua, getmetatable(e).obj.player)

e.yaw = getmetatable(e).obj.yaw

e.pitch = getmetatable(e).obj.pitch

e.entityId = getmetatable(e).obj.entityId

e.x = getmetatable(e).obj.x

e.y = getmetatable(e).obj.y

e.z = getmetatable(e).obj.z

e.face = getmetatable(e).obj:GetFace()

return e
")[0];
        }
    }
}
