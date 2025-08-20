import api from '@/services/api';

export const getExpensesByClub = (clubId) => {
  return api.get(`/club-expenses/club/${clubId}`);
};

export const addExpense = (payload) => {
  return api.post(`/club-expenses`, payload);
};

// Function to update an existing expense
export const updateExpense = (id, payload) => {
  return api.put(`/club-expenses/${id}`, payload);
};

export const deleteExpense = (id) => {
  return api.delete(`/club-expenses/${id}`);
};