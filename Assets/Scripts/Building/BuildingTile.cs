namespace Assets.Scripts.Building
{
    internal class BuildingTile
    {
        public BuildingTile(int tileX, int tileY)
        {
            this.TileX = tileX;
            this.TileY = tileY;
        }

        public int TileX { get; }

        public int TileY { get; }

        public bool IsOccupied { get; set; }

        public bool IsTopBorderOccupied { get; set; }

        public bool IsRightBorderOccupied { get; set; }

        public bool IsBottomBorderOccupied { get; set; }

        public bool IsLeftBorderOccupied;
    }
}
