<template>
  <div class="statistics-container">
    <div class="dashboard-header">
      <h1 class="page-title">Topluluk İstatistikleri</h1>
      
      <div class="date-filter">
        <label for="date-range">Tarih Aralığı:</label>
        <select id="date-range" v-model="selectedDateRange" class="filter-select">
          <option value="all">Tüm Zamanlar</option>
          <option value="today">Bugün</option>
          <option value="week">Bu Hafta</option>
          <option value="month">Bu Ay</option>
          <option value="year">Bu Yıl</option>
          <option value="custom">Özel Aralık</option>
        </select>
        
        <div v-if="selectedDateRange === 'custom'" class="custom-date-range">
          <input type="date" v-model="startDate" class="date-input" />
          <span>-</span>
          <input type="date" v-model="endDate" class="date-input" />
          <button @click="applyDateFilter" class="apply-btn">Uygula</button>
        </div>
      </div>
    </div>
    
    <div class="stats-cards">
      <!-- Özet Kartları -->
      <div class="stat-card">
        <div class="stat-icon">
          <i class="fas fa-users"></i>
        </div>
        <div class="stat-content">
          <h3>Toplam Üye Sayısı</h3>
          <div v-if="isLoading.summary" class="loading-placeholder"></div>
          <template v-else>
            <p class="stat-number">{{ totalMembers }}</p>
          </template>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon">
          <i class="fas fa-user-friends"></i>
        </div>
        <div class="stat-content">
          <h3>Toplam Topluluk Sayısı</h3>
          <div v-if="isLoading.summary" class="loading-placeholder"></div>
          <template v-else>
            <p class="stat-number">{{ totalClubs }}</p>
          </template>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon">
          <i class="fas fa-calendar-check"></i>
        </div>
        <div class="stat-content">
          <h3>Toplam Etkinlik Sayısı</h3>
          <div v-if="isLoading.summary" class="loading-placeholder"></div>
          <template v-else>
            <p class="stat-number">{{ totalEvents }}</p>
          </template>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon">
          <i class="fas fa-money-bill-wave"></i>
        </div>
        <div class="stat-content">
          <h3>Toplam Harcama (₺)</h3>
          <div v-if="isLoading.summary" class="loading-placeholder"></div>
          <template v-else>
            <p class="stat-number">{{ totalExpenses.toLocaleString('tr-TR') }}</p>
          </template>
        </div>
      </div>
    </div>

    <div class="charts-container">
      <div class="chart-tabs">
        <div class="tab-header">
          <button 
            v-for="(tab, index) in chartTabs" 
            :key="index" 
            :class="['tab-btn', { active: selectedChartTab === index }]"
            @click="selectedChartTab = index"
          >
            {{ tab.title }}
          </button>
        </div>
        <div class="tab-content">
          <div v-if="selectedChartTab === 0" class="chart-card">
            <h3 class="chart-title">Topluluk Üye Dağılımı</h3>
            <div class="chart-wrapper" v-if="!isLoading.clubs && topClubs.length > 0">
              <Pie :data="clubMemberChartData" :options="pieChartOptions" />
            </div>
            <div v-else-if="isLoading.clubs" class="loading-container">
                 <div class="loading-spinner"></div>
                 <p>Grafik yükleniyor...</p>
            </div>
            <div v-else class="chart-placeholder">
              <p>Üye dağılımı için veri bulunamadı veya topluluk yok.</p>
            </div>
          </div>

          <!-- En Aktif Topluluklar Tablosu -->
          <div v-if="selectedChartTab === 1" class="table-card">
            <div class="table-header">
              <h3 class="table-title">En Aktif Topluluklar</h3>
              <div class="table-actions">
                <div class="search-container">
                  <input type="text" placeholder="Ara..." v-model="clubSearchQuery" class="search-input" />
                  <i class="fas fa-search search-icon"></i>
                </div>
                <select v-model="clubsPerPage" class="items-per-page">
                  <option v-for="option in perPageOptions" :key="option" :value="option">
                    {{ option }} / sayfa
                  </option>
                </select>
              </div>
            </div>
            
            <div v-if="isLoading.clubs" class="loading-container">
              <div class="loading-spinner"></div>
              <p>Yükleniyor...</p>
            </div>
            
            <template v-else>
              <table class="data-table">
                <thead>
                  <tr>
                    <th @click="sortTable('topClubs', 'name')">
                      Topluluk Adı 
                      <i class="fas" :class="getSortIconClass('topClubs', 'name')"></i>
                    </th>
                    <th @click="sortTable('topClubs', 'memberCount')">
                      Üye Sayısı
                      <i class="fas" :class="getSortIconClass('topClubs', 'memberCount')"></i>
                    </th>
                    <th @click="sortTable('topClubs', 'eventCount')">
                      Etkinlik Sayısı
                      <i class="fas" :class="getSortIconClass('topClubs', 'eventCount')"></i>
                    </th>
                    <th @click="sortTable('topClubs', 'participationRate')">
                      Katılım Oranı
                      <i class="fas" :class="getSortIconClass('topClubs', 'participationRate')"></i>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(club, index) in paginatedClubs" :key="index">
                    <td>{{ club.name }}</td>
                    <td>{{ club.memberCount }}</td>
                    <td>{{ club.eventCount }}</td>
                    <td>{{ club.participationRate }}%</td>
                  </tr>
                  <tr v-if="paginatedClubs.length === 0">
                    <td colspan="4" class="no-data">Veri bulunamadı</td>
                  </tr>
                </tbody>
              </table>

              <div class="pagination">
                <button 
                  @click="clubCurrentPage = 1" 
                  :disabled="clubCurrentPage === 1"
                  class="pagination-btn"
                >
                  <i class="fas fa-angle-double-left"></i>
                </button>
                <button 
                  @click="clubCurrentPage--" 
                  :disabled="clubCurrentPage === 1"
                  class="pagination-btn"
                >
                  <i class="fas fa-angle-left"></i>
                </button>
                
                <span class="pagination-info">
                  Sayfa {{ clubCurrentPage }} / {{ totalClubPages }}
                </span>
                
                <button 
                  @click="clubCurrentPage++" 
                  :disabled="clubCurrentPage === totalClubPages"
                  class="pagination-btn"
                >
                  <i class="fas fa-angle-right"></i>
                </button>
                <button 
                  @click="clubCurrentPage = totalClubPages" 
                  :disabled="clubCurrentPage === totalClubPages"
                  class="pagination-btn"
                >
                  <i class="fas fa-angle-double-right"></i>
                </button>
              </div>
            </template>
          </div>

          <!-- Yıllık Etkinlik Raporu -->
          <div v-if="selectedChartTab === 2" class="table-card">
            <div class="table-header">
              <h3 class="table-title">Yıllık Etkinlik Raporu</h3>
              <div class="table-actions">
                <div class="filter-group">
                  <label for="year-select">Yıl:</label>
                  <select id="year-select" v-model="selectedYear" class="filter-select" @change="onYearChange">
                    <option v-for="year in availableYears" :key="year" :value="year">{{ year }}</option>
                  </select>
                </div>
                <div class="search-container">
                  <input type="text" placeholder="Ay Ara..." v-model="monthSearchQuery" class="search-input" />
                  <i class="fas fa-search search-icon"></i>
                </div>
                <select v-model="monthsPerPage" class="items-per-page">
                  <option v-for="option in perPageOptions" :key="option" :value="option">
                    {{ option }} / sayfa
                  </option>
                </select>
              </div>
            </div>
            
            <div v-if="isLoading.events" class="loading-container">
              <div class="loading-spinner"></div>
              <p>Yükleniyor...</p>
            </div>
            
            <template v-else>
              <table class="data-table">
                <thead>
                  <tr>
                    <th @click="sortTable('yearlyEvents', 'name')">
                      Ay
                      <i class="fas" :class="getSortIconClass('yearlyEvents', 'name')"></i>
                    </th>
                    <th @click="sortTable('yearlyEvents', 'eventCount')">
                      Etkinlik Sayısı
                      <i class="fas" :class="getSortIconClass('yearlyEvents', 'eventCount')"></i>
                    </th>
                    <th @click="sortTable('yearlyEvents', 'totalParticipants')">
                      Toplam Katılımcı
                      <i class="fas" :class="getSortIconClass('yearlyEvents', 'totalParticipants')"></i>
                    </th>
                    <th @click="sortTable('yearlyEvents', 'averageParticipation')">
                      Ortalama Katılım
                      <i class="fas" :class="getSortIconClass('yearlyEvents', 'averageParticipation')"></i>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(month, index) in paginatedMonths" :key="index">
                    <td>{{ month.name }}</td>
                    <td>{{ month.eventCount }}</td>
                    <td>{{ month.totalParticipants }}</td>
                    <td>{{ month.averageParticipation }}</td>
                  </tr>
                  <tr v-if="paginatedMonths.length === 0">
                    <td colspan="4" class="no-data">Veri bulunamadı</td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr>
                    <td>Toplam</td>
                    <td>{{ totalEventCount }}</td>
                    <td>{{ totalParticipantsCount }}</td>
                    <td>{{ averageParticipationAll }}</td>
                  </tr>
                </tfoot>
              </table>

              <div class="pagination">
                <button 
                  @click="monthCurrentPage = 1" 
                  :disabled="monthCurrentPage === 1"
                  class="pagination-btn"
                >
                  <i class="fas fa-angle-double-left"></i>
                </button>
                <button 
                  @click="monthCurrentPage--" 
                  :disabled="monthCurrentPage === 1"
                  class="pagination-btn"
                >
                  <i class="fas fa-angle-left"></i>
                </button>
                
                <span class="pagination-info">
                  Sayfa {{ monthCurrentPage }} / {{ totalMonthPages }}
                </span>
                
                <button 
                  @click="monthCurrentPage++" 
                  :disabled="monthCurrentPage === totalMonthPages"
                  class="pagination-btn"
                >
                  <i class="fas fa-angle-right"></i>
                </button>
                <button 
                  @click="monthCurrentPage = totalMonthPages" 
                  :disabled="monthCurrentPage === totalMonthPages"
                  class="pagination-btn"
                >
                  <i class="fas fa-angle-double-right"></i>
                </button>
              </div>
            </template>
          </div>

          <!-- Harcama İstatistikleri -->
          <div v-if="selectedChartTab === 3" class="expense-section">
             <h2 class="section-title">Harcama İstatistikleri</h2>
      
            <!-- Topluluk Harcamaları Tablosu -->
            <div class="table-card">
              <div class="table-header">
                <h3 class="table-title">Topluluk Harcamaları Detayı</h3>
                <div class="table-actions">
                  <div class="filter-group">
                    <label for="club-select">Topluluk:</label>
                    <select id="club-select" v-model="selectedClubId" class="filter-select" @change="onClubChange">
                      <option value="all">Tüm Topluluklar</option>
                      <option v-for="club in clubs" :key="club.id" :value="club.id">{{ club.name }}</option>
                    </select>
                  </div>
                  
                  <div class="filter-group">
                    <label for="expense-date-range">Tarih:</label>
                    <select id="expense-date-range" v-model="expenseDateRange" class="filter-select">
                      <option value="all">Tüm Zamanlar</option>
                      <option value="month">Son 1 Ay</option>
                      <option value="quarter">Son 3 Ay</option>
                      <option value="year">Son 1 Yıl</option>
                      <option value="custom">Özel Aralık</option>
                    </select>
                  </div>
                  
                  <div v-if="expenseDateRange === 'custom'" class="custom-date-range">
                    <input type="date" v-model="expenseStartDate" class="date-input" />
                    <span>-</span>
                    <input type="date" v-model="expenseEndDate" class="date-input" />
                    <button @click="applyExpenseDateFilter" class="apply-btn">Uygula</button>
                  </div>
                  
                  <div class="search-container">
                    <input type="text" placeholder="Harcama Ara..." v-model="expenseSearchQuery" class="search-input" />
                    <i class="fas fa-search search-icon"></i>
                  </div>
                </div>
              </div>
              
              <div v-if="isLoading.expenses" class="loading-container">
                <div class="loading-spinner"></div>
                <p>Yükleniyor...</p>
              </div>
              
              <template v-else>
                <table class="data-table">
                  <thead>
                    <tr>
                      <th @click="sortTable('expenses', 'clubName')">Topluluk <i class="fas" :class="getSortIconClass('expenses', 'clubName')"></i></th>
                      <th @click="sortTable('expenses', 'date')">Tarih <i class="fas" :class="getSortIconClass('expenses', 'date')"></i></th>
                      <th @click="sortTable('expenses', 'description')">Açıklama <i class="fas" :class="getSortIconClass('expenses', 'description')"></i></th>
                      <th @click="sortTable('expenses', 'cashSupport')">Nakit Destek (₺) <i class="fas" :class="getSortIconClass('expenses', 'cashSupport')"></i></th>
                      <th @click="sortTable('expenses', 'inKindSupport')">Ayni Destek (₺) <i class="fas" :class="getSortIconClass('expenses', 'inKindSupport')"></i></th>
                      <th @click="sortTable('expenses', 'total')">Toplam (₺) <i class="fas" :class="getSortIconClass('expenses', 'total')"></i></th>
                      <th>Doküman</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(expense, index) in paginatedExpenses" :key="index">
                      <td>{{ expense.clubName }}</td>
                      <td>{{ formatDate(expense.date) }}</td>
                      <td>{{ expense.description }}</td>
                      <td>{{ expense.cashSupport.toLocaleString('tr-TR') }}</td>
                      <td>{{ expense.inKindSupport.toLocaleString('tr-TR') }}</td>
                      <td>{{ (expense.cashSupport + expense.inKindSupport).toLocaleString('tr-TR') }}</td>
                      <td>
                        <a v-if="expense.dokumanUrl" :href="expense.dokumanUrl" target="_blank" class="doc-link">
                          <i class="fas fa-file-download"></i>
                        </a>
                        <span v-else>-</span>
                      </td>
                    </tr>
                    <tr v-if="paginatedExpenses.length === 0">
                      <td colspan="7" class="no-data">Veri bulunamadı</td>
                    </tr>
                  </tbody>
                  <tfoot>
                    <tr>
                      <th colspan="3">Toplam</th>
                      <th>{{ totalCashSupport.toLocaleString('tr-TR') }}</th>
                      <th>{{ totalInKindSupport.toLocaleString('tr-TR') }}</th>
                      <th>{{ (totalCashSupport + totalInKindSupport).toLocaleString('tr-TR') }}</th>
                      <th></th>
                    </tr>
                  </tfoot>
                </table>
                
                <div class="pagination">
                  <button 
                    @click="expenseCurrentPage = 1" 
                    :disabled="expenseCurrentPage === 1"
                    class="pagination-btn"
                  >
                    <i class="fas fa-angle-double-left"></i>
                  </button>
                  <button 
                    @click="expenseCurrentPage--" 
                    :disabled="expenseCurrentPage === 1"
                    class="pagination-btn"
                  >
                    <i class="fas fa-angle-left"></i>
                  </button>
                  
                  <span class="pagination-info">
                    Sayfa {{ expenseCurrentPage }} / {{ totalExpensePages }}
                  </span>
                  
                  <button 
                    @click="expenseCurrentPage++" 
                    :disabled="expenseCurrentPage === totalExpensePages"
                    class="pagination-btn"
                  >
                    <i class="fas fa-angle-right"></i>
                  </button>
                  <button 
                    @click="expenseCurrentPage = totalExpensePages" 
                    :disabled="expenseCurrentPage === totalExpensePages"
                    class="pagination-btn"
                  >
                    <i class="fas fa-angle-double-right"></i>
                  </button>
                </div>
              </template>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="data-tables">
      <!-- This section is now part of the tabs. It can be removed or repurposed. -->
      <!-- For now, I'm commenting it out to avoid duplication. -->
      <!-- 
      <div class="table-card">
        ... En Aktif Topluluklar Tablosu ...
      </div>
      <div class="table-card">
        ... Yıllık Etkinlik Raporu ...
      </div>
      -->
    </div>

    <!-- This section is now part of the tabs. It can be removed or repurposed. -->
    <!-- For now, I'm commenting it out to avoid duplication. -->
    <!--
    <div class="expense-section">
      ... Harcama İstatistikleri ...
    </div>
    -->
  </div>
</template>

<script>
import axios from 'axios';
import { statisticsService } from '@/services';
import { Pie } from 'vue-chartjs';
import {
  Chart as ChartJS, 
  Title, 
  Tooltip, 
  Legend, 
  ArcElement,
  CategoryScale, 
  LinearScale 
} from 'chart.js';

ChartJS.register(Title, Tooltip, Legend, ArcElement, CategoryScale, LinearScale);

export default {
  name: 'AdminStatistics',
  components: {
    Pie
  },
  data() {
    return {
      // Filtre ve sayfalama
      selectedDateRange: 'all',
      startDate: '',
      endDate: '',
      clubSearchQuery: '',
      monthSearchQuery: '',
      expenseSearchQuery: '',
      expenseDateRange: 'all',
      expenseStartDate: '',
      expenseEndDate: '',
      
      // Sayfalama ayarları
      clubCurrentPage: 1,
      monthCurrentPage: 1,
      expenseCurrentPage: 1,
      clubsPerPage: 5,
      monthsPerPage: 5,
      expensesPerPage: 10,
      perPageOptions: [5, 10, 20, 50],
      
      // Sıralama ayarları
      sortFields: {
        topClubs: { field: 'memberCount', direction: 'desc' },
        yearlyEvents: { field: 'name', direction: 'asc' },
        expenses: { field: 'date', direction: 'desc' }
      },

      // Grafik sekmeleri
      selectedChartTab: 0,
      chartTabs: [
        { title: 'Üye Dağılımı' },
        { title: 'En Aktif Topluluklar' },
        { title: 'Yıllık Etkinlik Raporu' },
        { title: 'Harcama İstatistikleri' },
      ],

      // API verilerini tutacak değişkenler
      isLoading: {
        summary: false,
        clubs: false,
        events: false,
        expenses: false
      },
      
      // Özet verileri
      totalMembers: 0,
      totalClubs: 0,
      totalEvents: 0,
      totalExpenses: 0,
      growthRates: {
        members: 0,
        clubs: 0,
        events: 0,
        expenses: 0
      },

      // Topluluk verileri
      topClubs: [],

      // Etkinlik verileri
      selectedYear: new Date().getFullYear(),
      availableYears: [],
      yearlyEvents: [],

      // Harcama verileri
      clubs: [],
      selectedClubId: 'all',
      expenses: [],

      pieChartOptions: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'top',
          },
          tooltip: {
            callbacks: {
              label: function(tooltipItem) {
                let label = tooltipItem.label || '';
                if (label) {
                  label += ': ';
                }
                if (tooltipItem.parsed !== null) {
                  label += tooltipItem.parsed + ' üye';
                }
                return label;
              }
            }
          }
        }
      }
    }
  },
  computed: {
    // Filtrelenmiş ve sıralanmış topluluk listesi
    filteredClubs() {
      let filtered = [...this.topClubs];
      
      if (this.clubSearchQuery) {
        const query = this.clubSearchQuery.toLowerCase();
        filtered = filtered.filter(club => 
          club.name.toLowerCase().includes(query)
        );
      }
      
      // Sıralama
      const { field, direction } = this.sortFields.topClubs;
      filtered.sort((a, b) => {
        let comparison = 0;
        if (typeof a[field] === 'string') {
          comparison = a[field].localeCompare(b[field]);
        } else {
          comparison = a[field] - b[field];
        }
        return direction === 'asc' ? comparison : -comparison;
      });
      
      return filtered;
    },
    
    // Sayfalanmış topluluk listesi
    paginatedClubs() {
      const start = (this.clubCurrentPage - 1) * this.clubsPerPage;
      const end = start + this.clubsPerPage;
      return this.filteredClubs.slice(start, end);
    },
    
    // Toplam topluluk sayfası sayısı
    totalClubPages() {
      return Math.ceil(this.filteredClubs.length / this.clubsPerPage);
    },
    
    // Filtrelenmiş ve sıralanmış ay listesi
    filteredMonths() {
      let filtered = [...this.yearlyEvents];
      
      if (this.monthSearchQuery) {
        const query = this.monthSearchQuery.toLowerCase();
        filtered = filtered.filter(month => 
          month.name.toLowerCase().includes(query)
        );
      }
      
      // Sıralama
      const { field, direction } = this.sortFields.yearlyEvents;
      filtered.sort((a, b) => {
        let comparison = 0;
        if (typeof a[field] === 'string') {
          comparison = a[field].localeCompare(b[field]);
        } else {
          comparison = a[field] - b[field];
        }
        return direction === 'asc' ? comparison : -comparison;
      });
      
      return filtered;
    },
    
    // Sayfalanmış ay listesi
    paginatedMonths() {
      const start = (this.monthCurrentPage - 1) * this.monthsPerPage;
      const end = start + this.monthsPerPage;
      return this.filteredMonths.slice(start, end);
    },
    
    // Toplam ay sayfası sayısı
    totalMonthPages() {
      return Math.ceil(this.filteredMonths.length / this.monthsPerPage);
    },
    
    // Yıllık etkinlik özet verileri
    totalEventCount() {
      return this.yearlyEvents.reduce((sum, month) => sum + month.eventCount, 0);
    },
    
    totalParticipantsCount() {
      return this.yearlyEvents.reduce((sum, month) => sum + month.totalParticipants, 0);
    },
    
    averageParticipationAll() {
      if (this.totalEventCount === 0) return 0;
      return Math.round(this.totalParticipantsCount / this.totalEventCount);
    },
    
    // Filtrelenmiş ve sıralanmış harcama listesi
    filteredExpenses() {
      let filtered = [...this.expenses];
      
      // Topluluk filtresi
      if (this.selectedClubId !== 'all') {
        filtered = filtered.filter(expense => expense.clubId.toString() === this.selectedClubId);
      }
      
      // Arama filtresi
      if (this.expenseSearchQuery) {
        const query = this.expenseSearchQuery.toLowerCase();
        filtered = filtered.filter(expense => 
          expense.clubName.toLowerCase().includes(query) ||
          expense.description.toLowerCase().includes(query)
        );
      }
      
      // Sıralama
      const { field, direction } = this.sortFields.expenses;
      filtered.sort((a, b) => {
        let comparison = 0;
        if (field === 'total') {
          comparison = (a.cashSupport + a.inKindSupport) - (b.cashSupport + b.inKindSupport);
        } else if (typeof a[field] === 'string') {
          comparison = a[field].localeCompare(b[field]);
        } else {
          comparison = a[field] - b[field];
        }
        return direction === 'asc' ? comparison : -comparison;
      });
      
      return filtered;
    },
    
    // Sayfalanmış harcama listesi
    paginatedExpenses() {
      const start = (this.expenseCurrentPage - 1) * this.expensesPerPage;
      const end = start + this.expensesPerPage;
      return this.filteredExpenses.slice(start, end);
    },
    
    // Toplam harcama sayfası sayısı
    totalExpensePages() {
      return Math.ceil(this.filteredExpenses.length / this.expensesPerPage);
    },
    
    // Toplam harcama özet verileri
    totalCashSupport() {
      return this.filteredExpenses.reduce((sum, expense) => sum + expense.cashSupport, 0);
    },
    
    totalInKindSupport() {
      return this.filteredExpenses.reduce((sum, expense) => sum + expense.inKindSupport, 0);
    },

    clubMemberChartData() {
      if (!this.topClubs || this.topClubs.length === 0) {
        return {
          labels: [],
          datasets: []
        };
      }
      const backgroundColors = this.topClubs.map((_, index) => {
        const hue = (index * (360 / this.topClubs.length)) % 360;
        return `hsl(${hue}, 70%, 60%)`;
      });
      return {
        labels: this.topClubs.map(club => club.name),
        datasets: [
          {
            label: 'Üye Sayısı',
            backgroundColor: backgroundColors,
            data: this.topClubs.map(club => club.memberCount)
          }
        ]
      };
    }
  },
  methods: {
    fetchStatistics() {
      this.fetchSummary();
      this.fetchClubStatistics();
      this.fetchEventStatistics();
      this.fetchExpenseStatistics();
    },
    async fetchSummary() {
      this.isLoading.summary = true;
      try {
        const response = await statisticsService.getSummaryStatistics();
        this.totalMembers = response.data.totalMembers;
        this.totalClubs = response.data.totalClubs;
        this.totalEvents = response.data.totalEvents;
        this.totalExpenses = response.data.totalExpenses;
        this.growthRates = response.data.growth || { members: 0, clubs: 0, events: 0, expenses: 0 }; 
      } catch (error) {
        console.error('Özet verileri alınırken hata oluştu:', error);
      } finally {
        this.isLoading.summary = false;
      }
    },
    async fetchClubStatistics() {
      this.isLoading.clubs = true;
      try {
        const response = await statisticsService.getClubStatistics();
        this.topClubs = response.data;
      } catch (error) {
        console.error('Topluluk istatistikleri alınırken hata oluştu:', error);
      } finally {
        this.isLoading.clubs = false;
      }
    },
    async fetchEventStatistics() {
      this.isLoading.events = true;
      try {
        const response = await statisticsService.getEventStatistics(this.selectedYear);
        this.yearlyEvents = response.data.monthlyStats;
        this.availableYears = response.data.availableYears;
      } catch (error) {
        console.error('Etkinlik istatistikleri alınırken hata oluştu:', error);
      } finally {
        this.isLoading.events = false;
      }
    },
    async fetchExpenseStatistics() {
      this.isLoading.expenses = true;
      try {
        let params = {
          clubId: this.selectedClubId,
          dateRange: this.expenseDateRange
        };
        
        if (this.expenseDateRange === 'custom' && this.expenseStartDate && this.expenseEndDate) {
          params.startDate = this.expenseStartDate;
          params.endDate = this.expenseEndDate;
        }
        
        const response = await statisticsService.getExpenseStatistics(params.clubId, params.dateRange, params.startDate, params.endDate);
        this.expenses = response.data.expenses;
        this.clubs = response.data.clubs;
      } catch (error) {
        console.error('Harcama istatistikleri alınırken hata oluştu:', error);
      } finally {
        this.isLoading.expenses = false;
      }
    },
    sortTable(tableType, field) {
      const sortInfo = this.sortFields[tableType];
      
      if (sortInfo.field === field) {
        // Aynı alan için sıralama yönünü değiştir
        sortInfo.direction = sortInfo.direction === 'asc' ? 'desc' : 'asc';
      } else {
        // Farklı alan için varsayılan sıralama yönü
        sortInfo.field = field;
        sortInfo.direction = 'asc';
      }
    },
    getSortIconClass(tableType, field) {
      const sortInfo = this.sortFields[tableType];
      if (sortInfo.field !== field) return '';
      
      return sortInfo.direction === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    },
    applyDateFilter() {
      // Global tarih filtresini uygula ve tüm verileri yeniden yükle
      this.fetchStatistics();
    },
    applyExpenseDateFilter() {
      // Harcama tarih filtresini uygula
      this.fetchExpenseStatistics();
    },
    onYearChange() {
      this.fetchEventStatistics();
    },
    onClubChange() {
      this.fetchExpenseStatistics();
    },
    formatDate(date) {
      // Tarih formatlama işlemi burada yapılabilir
      return date.split('-').reverse().join('/');
    },
    formatDateForFile(date) {
      return `${date.getDate()}-${date.getMonth() + 1}-${date.getFullYear()}`;
    }
  },
  created() {
    this.fetchStatistics();
  },
  watch: {
    selectedYear() {
      this.fetchEventStatistics();
    },
    selectedClubId() {
      this.fetchExpenseStatistics();
    },
    expenseDateRange() {
      if (this.expenseDateRange !== 'custom') {
        this.fetchExpenseStatistics();
      }
    }
  }
}
</script>

<style scoped>
.statistics-container {
  padding: 1.5rem;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
  gap: 1rem;
}

.page-title {
  margin: 0;
  color: #2c3e50;
  font-size: 1.8rem;
}

.date-filter {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.filter-select {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background-color: white;
  min-width: 150px;
}

.custom-date-range {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.date-input {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background-color: white;
}

.apply-btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  background-color: #3498db;
  color: white;
  cursor: pointer;
}

.stats-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 1.5rem;
  display: flex;
  align-items: center;
  transition: transform 0.2s, box-shadow 0.2s;
}

.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.stat-icon {
  background-color: #3498db;
  color: white;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  margin-right: 1rem;
}

.stat-content h3 {
  margin: 0 0 0.5rem 0;
  font-size: 1rem;
  color: #7f8c8d;
}

.stat-number {
  font-size: 1.8rem;
  font-weight: bold;
  color: #2c3e50;
  margin: 0;
}

.trend {
  font-size: 0.9rem;
  margin-top: 0.5rem;
  display: inline-block;
}

.trend.up {
  color: #2ecc71;
}

.trend.down {
  color: #e74c3c;
}

.charts-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(450px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.chart-tabs {
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.tab-header {
  display: flex;
  border-bottom: 1px solid #eee;
  overflow-x: auto;
}

.tab-btn {
  padding: 1rem 1.5rem;
  border: none;
  background: none;
  font-size: 1rem;
  cursor: pointer;
  white-space: nowrap;
  color: #7f8c8d;
  border-bottom: 2px solid transparent;
  transition: all 0.2s;
}

.tab-btn.active {
  color: #3498db;
  border-bottom: 2px solid #3498db;
  font-weight: 600;
}

.tab-btn:hover {
  background-color: #f8f9fa;
}

.chart-card {
  padding: 1.5rem;
}

.chart-title {
  margin: 0 0 1rem 0;
  color: #2c3e50;
  font-size: 1.2rem;
}

.chart-wrapper {
  height: 350px;
  position: relative;
  max-width: 400px;
  margin: auto;
}

.chart-placeholder {
  height: 300px;
  background-color: #f8f9fa;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #7f8c8d;
}

.data-tables {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(450px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.table-card {
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 1.5rem;
  overflow: hidden;
}

.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  flex-wrap: wrap;
  gap: 1rem;
}

.table-title {
  margin: 0;
  color: #2c3e50;
  font-size: 1.2rem;
}

.table-actions {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex-wrap: wrap;
}

.search-container {
  position: relative;
}

.search-input {
  padding: 0.5rem 0.75rem 0.5rem 2rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background-color: white;
}

.search-icon {
  position: absolute;
  left: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  color: #7f8c8d;
}

.items-per-page {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background-color: white;
}

.filter-group {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 1rem;
}

.data-table th,
.data-table td {
  padding: 0.75rem;
  text-align: left;
  border-bottom: 1px solid #eee;
}

.data-table th {
  font-weight: 600;
  color: #2c3e50;
  background-color: #f8f9fa;
  cursor: pointer;
  user-select: none;
  position: relative;
}

.data-table th:hover {
  background-color: #e9ecef;
}

.data-table th i {
  margin-left: 0.5rem;
  font-size: 0.8rem;
}

.data-table tbody tr:hover {
  background-color: #f8f9fa;
}

.data-table tfoot {
  font-weight: bold;
}

.data-table tfoot td,
.data-table tfoot th {
  border-top: 2px solid #ddd;
  border-bottom: none;
}

.no-data {
  text-align: center;
  color: #7f8c8d;
  padding: 2rem;
}

.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  margin-top: 1rem;
}

.pagination-btn {
  width: 36px;
  height: 36px;
  border: 1px solid #ddd;
  border-radius: 4px;
  background-color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.pagination-btn:hover:not(:disabled) {
  background-color: #f8f9fa;
}

.pagination-info {
  padding: 0 1rem;
  color: #7f8c8d;
}

.expense-section {
  margin-top: 2rem;
}

.section-title {
  margin-bottom: 1rem;
  color: #2c3e50;
  font-size: 1.2rem;
  border-bottom: 1px solid #eee;
  padding-bottom: 0.5rem;
}

.doc-link {
  color: #3498db;
  text-decoration: none;
  font-size: 1.2rem;
}

.doc-link:hover {
  color: #2980b9;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 0;
  color: #7f8c8d;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(0, 0, 0, 0.1);
  border-left-color: #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

.loading-placeholder {
  height: 24px;
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: loading-pulse 1.5s ease-in-out infinite;
  border-radius: 4px;
  margin-bottom: 0.5rem;
  width: 80%;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

@keyframes loading-pulse {
  0% {
    background-position: 200% 0;
  }
  100% {
    background-position: -200% 0;
  }
}

@media (max-width: 768px) {
  .charts-container,
  .data-tables {
    grid-template-columns: 1fr;
  }
  
  .dashboard-header,
  .table-header {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .date-filter,
  .table-actions {
    width: 100%;
  }
  
  .export-buttons {
    flex-direction: column;
    width: 100%;
  }
  
  .export-btn {
    width: 100%;
    justify-content: center;
  }
}
</style> 