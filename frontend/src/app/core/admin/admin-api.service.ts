import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AdminStats {
  totalProducts: number;
  activeProducts: number;
  lowStockProducts: number;
  totalUsers: number;
  totalManufacturers: number;
  totalOrders: number;
  totalRevenueCAD: number;
}

export interface AdminProduct {
  productId: number;
  name: string;
  slug: string;
  basePrice: number;
  salePrice?: number;
  stock: number;
  isActive: boolean;
  categoryName: string;
  manufacturerId?: number;
  manufacturerName?: string;
  primaryImageUrl?: string;
  createdAt: string;
}

export interface UpdateProductPayload {
  name: string;
  description?: string;
  basePrice: number;
  salePrice?: number;
  stock: number;
  isActive: boolean;
  manufacturerId?: number;
  primaryImageUrl?: string;
}

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly base = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  getStats(): Observable<AdminStats> {
    return this.http.get<AdminStats>(`${this.base}/stats`);
  }

  getProducts(search?: string, manufacturerId?: number): Observable<AdminProduct[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (manufacturerId) params = params.set('manufacturerId', manufacturerId.toString());
    return this.http.get<AdminProduct[]>(`${this.base}/products`, { params });
  }

  updateProduct(id: number, dto: UpdateProductPayload): Observable<{ message: string; productId: number }> {
    return this.http.put<{ message: string; productId: number }>(`${this.base}/products/${id}`, dto);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/products/${id}`);
  }
}
