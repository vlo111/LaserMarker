namespace LaserMarker.DataAccess
{
    public class UserDataDto
    {
        public long Id { get; set; }

        public long Sequence { get; set; }

        public string Token { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string UrlSport { get; set; }

        public string BgImageName { get; set; }

        public string EzdImageName { get; set; }

        public string FullImageName { get; set; }

        public byte[] FullImage { get; set; }

        public double BgImageScale { get; set; }

        public long BgImagePosX { get; set; }

        public long BgImagePosY { get; set; }

        public double EzdImageScale { get; set; }

        public long EzdImagePosX { get; set; }

        public long EzdImagePosY { get; set; }

    }
}
