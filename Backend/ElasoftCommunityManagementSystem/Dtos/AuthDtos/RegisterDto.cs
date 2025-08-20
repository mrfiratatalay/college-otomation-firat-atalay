using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Dtos.AuthDtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [MinLength(10, ErrorMessage = "Telefon numarası en az 10 haneli olmalıdır.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Uyruk alanı zorunludur.")]
        public string Nationality { get; set; }

        [Required(ErrorMessage = "Cinsiyet alanı zorunludur.")]
        public string Gender { get; set; }

        public string? DisabilityStatus { get; set; }

        public string? SchoolNumber { get; set; }
    }
}
