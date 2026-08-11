import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DashboardStatsDto {
  totalProducts: number;
  activeProducts: number;
  lowStockProducts: number;
  pendingApprovals: number;
  totalSellers: number;
  activeSellers: number;
  totalUsers: number;
  totalCustomers: number;
  activeCustomers: number;
  totalOrders: number;
  totalRevenueCAD: number;
  totalManufacturers: number;
  totalCategories: number;
  recentProductAdds: number;
  recentSales: number;
  recentCustomerUpdates: number;
}

export interface DailyViewsDto   { date: string; views: number; }
export interface MonthlySalesDto { month: string; revenue: number; orderCount: number; }
export interface DailyRevenueDto { date: string; revenue: number; orderCount: number; }

export interface TopProductDto {
  productId: number;
  name: string;
  imageUrl: string | null;
  unitsSold: number;
  revenue: number;
}

export interface CategoryBreakdownDto {
  category: string;
  orderCount: number;
  revenue: number;
  percentage: number;
}

export interface DashboardRecentOrderDto {
  orderId: number;
  status: string;
  totalAmountCAD: number;
  currencyCode: string;
  displayTotal: number;
  buyerName: string;
  createdAt: string;
}

export type DashboardRange = '7d' | '30d' | 'month' | 'this-month' | 'custom';

export interface DashboardFilter {
  range: DashboardRange;
  from?: string;
  to?: string;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  private buildParams(filter?: DashboardFilter): HttpParams {
    let params = new HttpParams();
    if (!filter) return params.set('range', '30d');
    params = params.set('range', filter.range);
    if (filter.range === 'custom' && filter.from) params = params.set('from', filter.from);
    if (filter.range === 'custom' && filter.to)   params = params.set('to', filter.to);
    return params;
  }

  getStats(filter?: DashboardFilter): Observable<DashboardStatsDto> {
    return this.http.get<DashboardStatsDto>(`${this.apiUrl}/stats`, { params: this.buildParams(filter) });
  }

  getRevenue(filter?: DashboardFilter): Observable<DailyRevenueDto[]> {
    return this.http.get<DailyRevenueDto[]>(`${this.apiUrl}/revenue`, { params: this.buildParams(filter) });
  }

  getDailyViews(filter?: DashboardFilter): Observable<DailyViewsDto[]> {
    return this.http.get<DailyViewsDto[]>(`${this.apiUrl}/daily-views`, { params: this.buildParams(filter) });
  }

  getMonthlySales(): Observable<MonthlySalesDto[]> {
    return this.http.get<MonthlySalesDto[]>(`${this.apiUrl}/monthly-sales`);
  }

  getTopProducts(filter?: DashboardFilter, limit = 5): Observable<TopProductDto[]> {
    const params = this.buildParams(filter).set('limit', limit.toString());
    return this.http.get<TopProductDto[]>(`${this.apiUrl}/top-products`, { params });
  }

  getCategoryBreakdown(filter?: DashboardFilter): Observable<CategoryBreakdownDto[]> {
    return this.http.get<CategoryBreakdownDto[]>(`${this.apiUrl}/category-breakdown`, { params: this.buildParams(filter) });
  }

  getRecentOrders(limit = 5): Observable<DashboardRecentOrderDto[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<DashboardRecentOrderDto[]>(`${this.apiUrl}/recent-orders`, { params });
  }
}
