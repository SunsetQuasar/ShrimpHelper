module ShrimpHelperKrill

using ..Ahorn, Maple

@mapdef Entity "ShrimpHelper/BonkKrill" BonkKrill(x::Integer, y::Integer, left::Bool=false, alreadyHitOnce::Bool=false)

const placements = Ahorn.PlacementDict(
    "Bonkrill (Shrimp Helper)" => Ahorn.EntityPlacement(
        BonkKrill
    ),
)

theSprite = "krill/SC2023/ShrimpHelper/asset/idle_0"
theSprite2 = "krill/SC2023/ShrimpHelper/asset/idlefall_1"

function Ahorn.selection(entity::BonkKrill)
    x, y = Ahorn.position(entity)
    sprite = theSprite
    scaleX = get(entity.data, "left", false) ? 1 : -1

    return Ahorn.getSpriteRectangle(sprite, x, y, jx=0.5, jy=0.5, sx=scaleX, sy=1)
end


function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BonkKrill, room::Maple.Room)
    sprite = get(entity.data, "alreadyHitOnce", false) ? theSprite2 : theSprite 
    scaleX = get(entity.data, "left", false) ? 1 : -1
    Ahorn.drawSprite(ctx, sprite, 0, 0, jx=0.5, jy=0.5, sx=scaleX, sy=1)
   
end
end