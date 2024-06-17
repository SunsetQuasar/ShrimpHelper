local drawableSprite = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local drawableLine = require("structs.drawable_line")
local drawableRectangle = require("structs.drawable_rectangle")

local ShrimpHelperShreoGate = {}

ShrimpHelperShreoGate.name = "ShrimpHelper/ShreoGate"
ShrimpHelperShreoGate.color = {1.0, 1.0, 1.0, 0.5}
ShrimpHelperShreoGate.placements = {
    name = "shreo_gate",
    data = {
        width = 8,
        height = 8,
        ignoreRightShreo = false,
        openByDefault = false
    }
}

ShrimpHelperShreoGate.depth = 0

function ShrimpHelperShreoGate.sprite(room, entity)
    local spr = {}

    sprite = drawableSprite.fromTexture("characters/theoCrystal/SC2023/shrimphelper/gateIcon", entity)
    sprite:addPosition(entity.width / 2, entity.height / 2)
    
    spr = {
        sprite,
        drawableRectangle.fromRectangle("fill", entity.x, entity.y, entity.width, entity.height, {0.8, 0.6, 0.1, 0.4}, {1, 0.5, 0.0, 0.6})
    }

    return spr
end

return ShrimpHelperShreoGate