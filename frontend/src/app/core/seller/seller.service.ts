import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SellerProductDto {
  productId: number;
  name: string;
  slug?: string;
  description?: string;
  basePrice: number;
  salePrice?: number;
  stock: number;
  status: string;
  approvalNote?: string;
  categoryName: string;
  categoryId?: number;
  parentCategoryId?: number;
  manufacturerName?: string;
  manufacturerId?: number;
  primaryImageUrl?: string;
  images?: SellerProductImageDto[];
  viewCount: number;
  createdAt: string;
  colors?: string[];
  sizes?: string[];
}

export interface SellerProductImageDto {
  imageUrl: string;
  isPrimary: boolean;
}

export interface SellerProductVariantDto {
  color?: string;
  size?: string;
  sku?: string;
  priceDelta: number;
  stock: number;
}

export interface SellerVariantImageDto {
  imageId:   number;
  imageUrl:  string;
  isPrimary: boolean;
  sortOrder: number;
}

export interface SellerVariantDto {
  variantId:  number;
  color?:     string | null;
  size?:      string | null;
  sku?:       string | null;
  priceDelta: number;
  stock:      number;
  isActive:   boolean;
  isDefault:  boolean;
  images:     SellerVariantImageDto[];
}

export interface UpsertVariantWithIdDto {
  variantId?: number | null;
  color?:     string | null;
  size?:      string | null;
  sku?:       string | null;
  priceDelta: number;
  stock:      number;
  isActive:   boolean;
  isDefault:  boolean;
  imageUrls?: string[];
}

export interface BulkUpsertVariantsDto {
  variants: UpsertVariantWithIdDto[];
}

export interface CreateSellerProductDto {
  categoryId: number;
  manufacturerId?: number | null;
  name: string;
  description?: string;
  basePrice: number;
  salePrice?: number | null;
  stock: number;
  images?: SellerProductImageDto[];
  variants?: SellerProductVariantDto[];
  colors?: string[];
  sizes?: string[];
}

export interface UpdateSellerProductDto extends CreateSellerProductDto {}

export interface TopProductDto {
  productId: number;
  name: string;
  imageUrl?: string;
  thisMonthViews: number;
  lastMonthViews: number;
  percentageChange: number;
}

export interface SellerDashboardDto {
  totalProducts: number;
  activeProducts: number;
  lowStockProducts: number;
  totalOrders: number;
  totalRevenueCAD: number;
  recentProductAdds: number;
  recentSales: number;
  topProducts?: TopProductDto[];
}

export interface RevenueByDayDto {
  date: string;
  revenueCAD: number;
  orderCount: number;
}

export interface AnalyticsTopProductDto {
  productId: number;
  name: string;
  primaryImageUrl?: string | null;
  unitsSold: number;
  revenueCAD: number;
}

export interface SellerAnalyticsSummaryDto {
  totalSalesCAD: number;
  orderCount: number;
  unitsSold: number;
  averageOrderValue: number;
  conversionRate: number;
  totalViews: number;
  uniqueViewers: number;
  topProducts: AnalyticsTopProductDto[];
  revenueByDay: RevenueByDayDto[];
}

export interface SellerOrderListDto {
  orderId: number;
  status: string;
  buyerName: string;
  buyerEmail: string;
  createdAt: string;
  currencyCode: string;
  itemCount: number;
  sellerRevenueCAD: number;
}

export interface SellerOrderDetailDto {
  orderId: number;
  status: string;
  createdAt: string;
  updatedAt?: string;
  currencyCode: string;
  buyerName: string;
  buyerEmail: string;
  buyerPhone?: string;
  shippingAddress?: {
    fullName: string;
    phone: string;
    addressLine1: string;
    addressLine2?: string;
    city: string;
    state?: string;
    postalCode: string;
    country: string;
  };
  items: Array<{
    orderItemId: number;
    productId: number;
    variantId?: number;
    productName: string;
    primaryImageUrl?: string;
    variantInfo?: string;
    quantity: number;
    unitPriceCAD: number;
  }>;
  sellerRevenueCAD: number;
}

export type SellerDashboardRange = '7d' | '30d' | 'month' | 'this-month' | 'custom';
export interface SellerDashboardFilter {
  range: SellerDashboardRange;
  from?: string;
  to?: string;
}

@Injectable({ providedIn: 'root' })
export class SellerService {
  private base = `${environment.apiUrl}/seller`;

  constructor(private http: HttpClient) {}

  // ── Dashboard
  getDashboard(): Observable<SellerDashboardDto> {
    return this.http.get<SellerDashboardDto>(`${this.base}/dashboard`);
  }

  // ── Analytics
  getAnalyticsSummary(filter?: SellerDashboardFilter): Observable<SellerAnalyticsSummaryDto> {
    let params = new HttpParams().set('range', filter?.range ?? '30d');
    if (filter?.range === 'custom' && filter.from) params = params.set('from', filter.from);
    if (filter?.range === 'custom' && filter.to)   params = params.set('to',   filter.to);
    return this.http.get<SellerAnalyticsSummaryDto>(`${this.base}/analytics/summary`, { params });
  }

  // ── Products
  getProducts(search?: string, status?: string): Observable<SellerProductDto[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status);
    return this.http.get<SellerProductDto[]>(`${this.base}/products`, { params });
  }

  getProduct(id: number): Observable<SellerProductDto> {
    return this.http.get<SellerProductDto>(`${this.base}/products/${id}`);
  }

  createProduct(dto: CreateSellerProductDto): Observable<{ productId: number; status: string }> {
    return this.http.post<{ productId: number; status: string }>(`${this.base}/products`, dto);
  }

  updateProduct(id: number, dto: UpdateSellerProductDto): Observable<{ productId: number; status: string; message: string }> {
    return this.http.put<{ productId: number; status: string; message: string }>(`${this.base}/products/${id}`, dto);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/products/${id}`);
  }

  // ── Variants
  getVariants(productId: number): Observable<SellerVariantDto[]> {
    return this.http.get<SellerVariantDto[]>(`${this.base}/products/${productId}/variants`);
  }

  saveVariants(productId: number, dto: BulkUpsertVariantsDto): Observable<{ message: string; productId: number }> {
    return this.http.put<{ message: string; productId: number }>(`${this.base}/products/${productId}/variants`, dto);
  }

  // ── Orders
  getOrders(search?: string, status?: string): Observable<SellerOrderListDto[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status);
    return this.http.get<SellerOrderListDto[]>(`${this.base}/orders`, { params });
  }

  getOrder(id: number): Observable<SellerOrderDetailDto> {
    return this.http.get<SellerOrderDetailDto>(`${this.base}/orders/${id}`);
  }
}
