using Api.Entities;
using GoogleApi.Drive;

namespace Api.Models
{
    public class AvatarBase64
    {
        private readonly GoogleDriveApi driveApi = new GoogleDriveApi();

        public byte AvatarId { get; }
        public string Name { get; }
        public string Base64 { get; }

        public AvatarBase64(Avatar avatar)
        {
            AvatarId = avatar.AvatarId;
            Name = avatar.Name;
            Base64 = driveApi.DownloadFile(avatar.Uri).DataBase64;
        }
    }
}