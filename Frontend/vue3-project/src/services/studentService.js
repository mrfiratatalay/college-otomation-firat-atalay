import api from './api';

const studentService = {
    // Topluluk işlemleri
    getClubs: async () => {
        try {
            console.log('Topluluklar getiriliyor...');
            const response = await api.get('/clubs/listele');
            console.log('Topluluklar başarıyla getirildi:', response.data);
            return response;
        } catch (error) {
            console.error('Topluluklar getirilirken hata:', error);
            throw error;
        }
    },

    getClubDetails: async (clubId) => {
        try {
            console.log(`${clubId} ID'li topluluk detayları getiriliyor...`);
            const response = await api.get(`/clubs/${clubId}`);
            console.log('Topluluk detayları başarıyla getirildi:', response.data);
            return response;
        } catch (error) {
            console.error('Topluluk detayları getirilirken hata:', error);
            throw error;
        }
    },

    joinClub: async (clubId) => {
        try {
            console.log(`${clubId} ID'li topluluğa katılım isteği gönderiliyor...`);
            const response = await api.post(`/memberships/basvur`, { clubId: clubId });
            console.log('Topluluğa katılım isteği başarılı (backend yanıtı):', response.data);
            return response;
        } catch (error) {
            console.error('Topluluğa katılırken hata:', error);
            if (error.response) {
                console.error('Backend Hata Detayı:', error.response.data);
            }
            throw error;
        }
    },

    leaveClub: async (clubId) => {
        try {
            console.log(`${clubId} ID'li topluluktan ayrılma isteği gönderiliyor...`);
            const response = await api.post(`/student/clubs/${clubId}/leave`);
            console.log('Topluluktan ayrılma başarılı:', response.data);
            return response;
        } catch (error) {
            console.error('Topluluktan ayrılırken hata:', error);
            throw error;
        }
    },

    // Etkinlik işlemleri
    getEvents: async (clubId = null, search = null) => {
        try {
            console.log('Etkinlikler getiriliyor...');
            let url = '/events/listele';
            const params = new URLSearchParams();

            if (clubId) params.append('clubId', clubId);
            if (search) params.append('search', search);

            if (params.toString()) {
                url += '?' + params.toString();
            }

            const response = await api.get(url);
            console.log('Etkinlikler başarıyla getirildi:', response.data);
            return response;
        } catch (error) {
            console.error('Etkinlikler getirilirken hata:', error);
            throw error;
        }
    },

    getEventDetails: async (eventId) => {
        try {
            console.log(`${eventId} ID'li etkinlik detayları getiriliyor...`);
            const response = await api.get(`/events/${eventId}`);
            console.log('Etkinlik detayları başarıyla getirildi:', response.data);
            return response;
        } catch (error) {
            console.error('Etkinlik detayları getirilirken hata:', error);
            throw error;
        }
    },

    joinEvent: async (eventId) => {
        try {
            console.log(`${eventId} ID'li etkinliğe katılım isteği gönderiliyor...`);
            const response = await api.post(`/events/${eventId}/katil`);
            console.log('Etkinliğe katılım başarılı:', response.data);
            return response;
        } catch (error) {
            console.error('Etkinliğe katılırken hata:', error);
            throw error;
        }
    },

    leaveEvent: async (eventId) => {
        try {
            console.log(`${eventId} ID'li etkinlikten ayrılma isteği gönderiliyor...`);
            const response = await api.delete(`/events/${eventId}/ayril`);
            console.log('Etkinlikten ayrılma başarılı:', response.data);
            return response;
        } catch (error) {
            console.error('Etkinlikten ayrılırken hata:', error);
            throw error;
        }
    },

    checkEventParticipation: async (eventId) => {
        try {
            console.log(`${eventId} ID'li etkinlik katılım durumu kontrol ediliyor...`);
            const response = await api.get(`/events/${eventId}/check-participation`);
            console.log('Katılım durumu:', response.data);
            return response.data.isParticipating;
        } catch (error) {
            console.error('Katılım durumu kontrol edilirken hata:', error);
            throw error;
        }
    },

    // Duyuru işlemleri
    getAnnouncements: async () => {
        try {
            console.log('Duyurular getiriliyor...');
            const response = await api.get('/announcements');
            console.log('Duyurular başarıyla getirildi:', response.data);
            return response;
        } catch (error) {
            console.error('Duyurular getirilirken hata:', error);
            throw error;
        }
    },

    getAnnouncementDetails: async (announcementId) => {
        try {
            console.log(`${announcementId} ID'li duyuru detayları getiriliyor...`);
            const response = await api.get(`/announcements/${announcementId}`);
            console.log('Duyuru detayları başarıyla getirildi:', response.data);
            return response;
        } catch (error) {
            console.error('Duyuru detayları getirilirken hata:', error);
            throw error;
        }
    },

    // Profil işlemleri
    getProfile: async () => {
        try {
            console.log('Profil bilgileri getiriliyor...');
            const response = await api.get('/student/profile');
            console.log('Profil bilgileri başarıyla getirildi:', response.data);
            return response;
        } catch (error) {
            console.error('Profil bilgileri getirilirken hata:', error);
            throw error;
        }
    },

    updateProfile: async (profileData) => {
        try {
            console.log('Profil güncelleme isteği gönderiliyor:', profileData);
            const response = await api.put('/student/profile', profileData);
            console.log('Profil başarıyla güncellendi:', response.data);
            return response;
        } catch (error) {
            console.error('Profil güncellenirken hata:', error);
            throw error;
        }
    },

    // Dashboard verileri
    getDashboardData: async () => {
        try {
            console.log('Dashboard verileri getiriliyor...');
            const response = await api.get('/student/dashboard');
            console.log('Dashboard verileri başarıyla getirildi:', response.data);
            return response;
        } catch (error) {
            console.error('Dashboard verileri getirilirken hata:', error);
            throw error;
        }
    },

    // Kullanıcının üyelik durumlarını ve rollerini getir
    getMyMemberships: async () => {
        try {
            console.log('Kullanıcı üyelik bilgileri getiriliyor...');
            const response = await api.get('/memberships/whoami');
            console.log('Raw API Response:', response);
            console.log('Response Headers:', response.headers);
            console.log('Response Status:', response.status);
            console.log('Response Data:', JSON.stringify(response.data, null, 2));
            return {
                data: Array.isArray(response.data) ? response.data :
                    (response.data && Array.isArray(response.data.memberships) ? response.data.memberships :
                        [])
            };
        } catch (error) {
            console.error('Kullanıcı üyelik bilgileri getirilirken hata:', error);
            if (error.response) {
                console.error('Error Response:', error.response);
                console.error('Error Data:', error.response.data);
            }
            throw error;
        }
    },

    // Lider olunan kulüplerin bekleyen başvurularını getir
    getPendingApplicationsForMyLedClubs: async () => {
        try {
            console.log('Lider olunan kulüplerin başvuruları getiriliyor...');
            const response = await api.get('/memberships/my-led-clubs/pending-applications');
            console.log('Lider olunan kulüplerin başvuruları başarıyla getirildi:', response.data);
            return response.data;
        } catch (error) {
            console.error('Lider olunan kulüplerin başvuruları getirilirken hata:', error);
            throw error;
        }
    },

    // Bir üyeliği onayla (Lider yetkisi gerekir)
    approveMembership: async (membershipId) => {
        try {
            console.log(`${membershipId} ID'li üyelik onaylanıyor...`);
            const response = await api.put(`memberships/${membershipId}/basvuruonaylama`);
            console.log('Üyelik başarıyla onaylandı:', response.data);
            return response.data;
        } catch (error) {
            console.error('Üyelik onaylanırken hata:', error);
            throw error;
        }
    },

    // Bir üyeliği reddet (Lider yetkisi gerekir)
    rejectMembership: async (membershipId) => {
        try {
            console.log(`${membershipId} ID'li üyelik reddediliyor...`);
            const response = await api.delete(`memberships/${membershipId}/basvurureddet`);
            console.log('Üyelik başarıyla reddedildi:', response.data);
            return response.data;
        } catch (error) {
            console.error('Üyelik reddedilirken hata:', error);
            throw error;
        }
    },

    async getMembersForMyLedClubs(clubId) {
        try {
            console.log(`${clubId} ID'li topluluğun üyeleri getiriliyor...`);
            const response = await api.get(`/memberships/club/${clubId}/members`);
            console.log('Topluluk üyeleri başarıyla getirildi:', response.data);
            return response.data;
        } catch (error) {
            console.error('Topluluk üyeleri alınırken hata:', error);
            if (error.response) {
                console.error('Error Response:', error.response);
                console.error('Error Data:', error.response.data);
            }
            throw error;
        }
    },

    // Kullanıcının başvurularını getir
    getMyClubApplications: async () => {
        try {
            console.log('Kullanıcı başvuruları getiriliyor...');
            const response = await api.get('/memberships/my-club/basvurular');
            console.log('Kullanıcı başvuruları başarıyla getirildi:', response.data);
            return response.data;
        } catch (error) {
            console.error('Kullanıcı başvuruları getirilirken hata:', error);
            throw error;
        }
    },

    // Bir üyeyi topluluktan çıkar (Lider yetkisi gerekir)
    removeMember: async (membershipId) => {
        try {
            console.log(`${membershipId} ID'li üye çıkarılıyor...`);
            const response = await api.delete(`/memberships/${membershipId}/remove`);
            console.log('Üye başarıyla çıkarıldı:', response.data);
            return response.data;
        } catch (error) {
            console.error('Üye çıkarılırken hata:', error);
            throw error;
        }
    },

    // Onaylı üyeyi sil
    async removeClubMember(membershipId) {
        try {
            console.log(`${membershipId} ID'li üye siliniyor...`);
            const response = await api.delete(`/memberships/approved-member/${membershipId}`);
            console.log('Üye başarıyla silindi:', response.data);
            return response.data;
        } catch (error) {
            console.error('Üye silinirken hata:', error);
            if (error.response) {
                console.error('Error Response:', error.response);
                console.error('Error Data:', error.response.data);
            }
            throw error;
        }
    },

    // Üye detaylarını getir
    async getMemberDetails(userId) {
        try {
            console.log(`${userId} ID'li üyenin detayları getiriliyor...`);
            const response = await api.get(`/memberships/member-details/${userId}`);
            console.log('Üye detayları başarıyla getirildi:', response.data);
            return response.data;
        } catch (error) {
            console.error('Üye detayları alınırken hata:', error);
            if (error.response) {
                console.error('Error Response:', error.response);
                console.error('Error Data:', error.response.data);
            }
            throw error;
        }
    },

    /**
     * Topluluktan ayrılma
     * @param {number} membershipId - Üyelik ID'si
     * @returns {Promise} - API yanıtı
     */
    leaveClub(membershipId) {
        try {
            console.log(`${membershipId} ID'li üyelikten ayrılma isteği gönderiliyor...`);
            return api.delete(`/memberships/approved-member/${membershipId}/leave`);
        } catch (error) {
            console.error('Topluluktan ayrılırken hata:', error);
            if (error.response) {
                console.error('Error Response:', error.response);
                console.error('Error Data:', error.response.data);
            }
            throw error;
        }
    }
};

export default studentService; 