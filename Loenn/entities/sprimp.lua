local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local ShrimpHelperSprimp = {}

ShrimpHelperSprimp.name = "ShrimpHelper/Sprimp"
ShrimpHelperSprimp.depth = -5
ShrimpHelperSprimp.placements = {
    {
        name = "right",
        data = {
            oneUse = false,
            left = false
        }
    },
    {
        name = "left",
        data = {
            oneUse = false,
            left = true
        }
    }
}

--local texture = "sprimp/SC2023/ShrimpHelper/asset/dissipate"

function ShrimpHelperSprimp.sprite(room, entity)
    if entity.left then 
        return drawableSprite.fromTexture("sprimp/SC2023/ShrimpHelper/asset/loennOnlyThingImTooLazy", entity)
    else
        return drawableSprite.fromTexture("sprimp/SC2023/ShrimpHelper/asset/dissipate", entity)
    end
    
end

function ShrimpHelperSprimp.rectangle(room, entity)
    local sprite = drawableSprite.fromTexture("sprimp/SC2023/ShrimpHelper/asset/dissipate", entity)

    return sprite:getRectangle()
end

return ShrimpHelperSprimp