namespace SaberFactory.Models.PropHandler
{
    public class WhackerPropertyBlock : PiecePropertyBlock
    {
        public override void SyncFrom(PiecePropertyBlock otherBlock)
        {
            var block = (WhackerPropertyBlock)otherBlock;
            TransformProperty.Width = block.TransformProperty.Width;
            TransformProperty.Rotation = -block.TransformProperty.Rotation;
            TransformProperty.Offset = block.TransformProperty.Offset;
        }
    }
}