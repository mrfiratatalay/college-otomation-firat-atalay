public class ReservationDto
{
    public string LocationName { get; set; }     // "tesis" → LocationName
    public string StartHour { get; set; }        // Saatin başlangıcı
    public string EndHour { get; set; }          // Saatin bitişi
    public string ClubName { get; set; }         // "kulup" → ClubName
    public string EventDate { get; set; }        // "tarih" → EventDate (örn: 25/07/2025)
}
