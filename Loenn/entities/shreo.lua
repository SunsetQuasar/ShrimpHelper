local drawableSprite = require("structs.drawable_sprite")

local ShrimpHelperShreoCrystal = {}

ShrimpHelperShreoCrystal.name = "ShrimpHelper/Shreo"
ShrimpHelperShreoCrystal.depth = 100
ShrimpHelperShreoCrystal.placements = {
    name = "theo_crystal",
    data = {
        texture = "characters/theoCrystal/SC2023/ShrimpHelper/shreo",
        TagFix = true,
        removeDuplicates = true,
        tutorial = false,
        killPlayer = true,
    }
}

-- Offset is from sprites.xml, not justifications
local offsetY = -10


function ShrimpHelperShreoCrystal.sprite(room, entity)
    local texture = entity.texture
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end

return ShrimpHelperShreoCrystal