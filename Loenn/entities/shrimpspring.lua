local drawableSprite = require("structs.drawable_sprite")

local springDepth = -8501

local ShrimpHelperSpring = {}

ShrimpHelperSpring.name = "ShrimpHelper/KrillSpring"
ShrimpHelperSpring.depth = springDepth
ShrimpHelperSpring.justification = {0.5, 1.0}
ShrimpHelperSpring.texture = springTexture
ShrimpHelperSpring.placements = {
    name = "down",
    data = {
        playerCanUse = true,
        orientation = 3
    }
}

function ShrimpHelperSpring.rotate(room, entity, direction)
    if (entity.orientation == 0) then
        if direction > 0 then
            entity.orientation = 1
        else
            entity.orientation = 2
        end
    elseif (entity.orientation == 1) then
        if direction > 0 then
            entity.orientation = 3
        else
            entity.orientation = 0
        end
    elseif (entity.orientation == 2) then
        if direction > 0 then
            entity.orientation = 0
        else
            entity.orientation = 3
        end
    else
        if direction > 0 then
            entity.orientation = 2
        else
            entity.orientation = 1
        end
    end


    return true
end

function ShrimpHelperSpring.flip(room, entity, horizontal, vertical)
    if (entity.orientation == 0 or entity.orientation == 3) then
        if (entity.orientation == 0) then
            entity.orientation = 3
        else 
            entity.orientation = 0
        end
    elseif (entity.orientation == 1 or entity.orientation == 2) then
        if (entity.orientation == 1) then
            entity.orientation = 2
        else
            entity.orientation = 1
        end
    else 
        return false
    end

    return true
end

function ShrimpHelperSpring.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/spring/00", entity)
    sprite:setJustification(0.5, 1.0)
    if (entity.orientation == 1) then
        sprite.rotation = math.pi / 2
    elseif (entity.orientation == 2) then
        sprite.rotation = -math.pi / 2
    elseif (entity.orientation == 3) then
        sprite.rotation = math.pi
    else
        sprite.rotation = 0
    end

    return sprite
end

return ShrimpHelperSpring
