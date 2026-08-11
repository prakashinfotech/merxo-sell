import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  PaginatedResponse,
  ProductDetail,
  ProductFilter,
  ProductListItem,
  ProductSearchFilter,
  SuggestItem
} from '../../shared/models/product.models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly base = `${environment.apiUrl}/products`;

  constructor(private http: HttpClient) { }

  getAll(filter: ProductFilter = {}): Observable<PaginatedResponse<ProductListItem>> {
    let params = new HttpParams();
    if (filter.categoryId != null) params = params.set('categoryId', filter.categoryId);
    if (filter.search) params = params.set('search', filter.search);
    if (filter.minPrice != null) params = params.set('minPrice', filter.minPrice);
    if (filter.maxPrice != null) params = params.set('maxPrice', filter.maxPrice);
    if (filter.minRating != null) params = params.set('minRating', filter.minRating);
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);
    if (filter.page != null) params = params.set('page', filter.page);
    if (filter.pageSize != null) params = params.set('pageSize', filter.pageSize);
    return this.http.get<PaginatedResponse<ProductListItem>>(this.base, { params });
  }

  getById(id: number): Observable<ProductDetail> {
    return this.http.get<ProductDetail>(`${this.base}/${id}`);
  }

  /** GET /api/products/search */
  search(filter: ProductSearchFilter = {}): Observable<PaginatedResponse<ProductListItem>> {
    let params = new HttpParams();
    if (filter.q)              params = params.set('q', filter.q);
    if (filter.categoryId != null) params = params.set('categoryId', filter.categoryId);
    if (filter.sellerId != null)   params = params.set('sellerId', filter.sellerId);
    if (filter.minPrice != null)   params = params.set('minPrice', filter.minPrice);
    if (filter.maxPrice != null)   params = params.set('maxPrice', filter.maxPrice);
    if (filter.inStock != null)    params = params.set('inStock', filter.inStock);
    if (filter.sortBy)         params = params.set('sortBy', filter.sortBy);
    if (filter.page != null)   params = params.set('page', filter.page);
    if (filter.pageSize != null) params = params.set('pageSize', filter.pageSize);
    return this.http.get<PaginatedResponse<ProductListItem>>(`${this.base}/search`, { params });
  }

  /** GET /api/products/suggest?q= */
  suggest(query: string): Observable<SuggestItem[]> {
    const params = new HttpParams().set('q', query);
    return this.http.get<SuggestItem[]>(`${this.base}/suggest`, { params });
  }
}
