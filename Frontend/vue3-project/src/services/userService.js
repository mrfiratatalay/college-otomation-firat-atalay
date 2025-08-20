import api from './api';

const userService = {
    async changeUserRole(userId, role) {
        try {
            const response = await api.put(`/users/${userId}/role`, { role });
            return response.data;
        } catch (error) {
            throw error;
        }
    },

    async getDepartmentDistribution(clubId) {
        try {
            // Assuming the endpoint can filter by clubId via query parameter
            const response = await api.get(`/users/department-distribution?clubId=${clubId}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching department distribution:', error);
            throw error;
        }
    }


};


export default userService; 