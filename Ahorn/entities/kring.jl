module ShrimpHelperKrillSpring

using ..Ahorn, Maple

@mapdef Entity "ShrimpHelper/KrillSpring" KrillSpring(x::Integer, y::Integer, orientation::Integer=3)

const placements = Ahorn.PlacementDict(
    "Spring (Down, Bonkrill Only, Shrimp Helper)" => Ahorn.EntityPlacement(
        KrillSpring
    ),
)

function Ahorn.selection(entity::KrillSpring)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 6, y - 1, 12, 5)
end

sprite = "objects/spring/00.png"

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KrillSpring, room::Maple.Room) 
    Ahorn.drawSprite(ctx, sprite, 12, -2, rot=pi)
end
end
