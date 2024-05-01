module ShrimpHelperSprimp

using ..Ahorn, Maple

@mapdef Entity "ShrimpHelper/Sprimp" Sprimp(x::Integer, y::Integer, left::Bool=false, oneUse::Bool=false)

const placements = Ahorn.PlacementDict(
    "Sprimp (Shrimp Helper)" => Ahorn.EntityPlacement(
        Sprimp
    ),
)

theSprite = "sprimp/SC2023/ShrimpHelper/asset/idle_0"

function Ahorn.selection(entity::Sprimp)
    x, y = Ahorn.position(entity)
    sprite = theSprite
    scaleX = get(entity.data, "left", false) ? -1 : 1

    return Ahorn.getSpriteRectangle(sprite, x, y, jx=0.5, jy=0.5, sx=scaleX, sy=1)
end


function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Sprimp, room::Maple.Room)
    sprite = theSprite
    scaleX = get(entity.data, "left", false) ? -1 : 1
    Ahorn.drawSprite(ctx, sprite, 0, 0, jx=0.5, jy=0.5, sx=scaleX, sy=1)
   
end
end