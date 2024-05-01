local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local ShrimpHelperBonkKrill = {}

ShrimpHelperBonkKrill.name = "ShrimpHelper/BonkKrill"
ShrimpHelperBonkKrill.depth = -5
ShrimpHelperBonkKrill.placements = {
    {
        name = "right",
        data = {
            alreadyHitOnce = false,
            left = false,
            moreInteractions = true
        }
    },
    {
        name = "rightHitOnce",
        data = {
            alreadyHitOnce = true,
            left = false,
            moreInteractions = true
        }
    },
    {
        name = "leftHitOnce",
        data = {
            alreadyHitOnce = true,
            left = true,
            moreInteractions = true
        }
    },
    {
        name = "left",
        data = {
            alreadyHitOnce = false,
            left = true,
            moreInteractions = true
        }
    }
}

--local texture = "sprimp/SC2023/ShrimpHelper/asset/dissipate"

function ShrimpHelperBonkKrill.sprite(room, entity)
    if entity.alreadyHitOnce == false then
        if entity.left then 
            return drawableSprite.fromTexture("krill/SC2023/ShrimpHelper/asset/idle_0", entity)
        else
            return drawableSprite.fromTexture("krill/SC2023/ShrimpHelper/asset/loennThingImSorry", entity)
        end
    else
        if entity.left then 
            return drawableSprite.fromTexture("krill/SC2023/ShrimpHelper/asset/idlefall_0", entity)
        else
            return drawableSprite.fromTexture("krill/SC2023/ShrimpHelper/asset/loennThingImSorry2", entity)
        end
    end
end

function ShrimpHelperBonkKrill.rectangle(room, entity)
    local sprite = drawableSprite.fromTexture("krill/SC2023/ShrimpHelper/asset/idle_0", entity)

    return sprite:getRectangle()
end

return ShrimpHelperBonkKrill