import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ReviewDto {
  reviewId: number;
  productId: number;
  userId: number;
  userName: string;
  rating: number;
  comment?: string;
  createdAt: string;
}

export interface CreateReviewDto {
  productId: number;
  rating: number;
  comment?: string;
}

export interface UpdateReviewDto {
  rating: number;
  comment?: string;
}

/** Returned by GET /api/reviews/me — denormalised with product info for the profile page. */
export interface MyReviewDto {
  reviewId: number;
  productId: number;
  productName: string;
  productImageUrl?: string;
  rating: number;
  comment?: string;
  createdAt: string;
}

export interface PortalReviewDto {
  reviewId: number;
  productId: number;
  productName: string;
  userId: number;
  userName: string;
  userEmail: string;
  rating: number;
  comment?: string;
  createdAt: string;
  sellerId?: number;
  storeName?: string;
}

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private apiUrl = `${environment.apiUrl}/reviews`;

  constructor(private http: HttpClient) {}

  addReview(dto: CreateReviewDto): Observable<ReviewDto> {
    return this.http.post<ReviewDto>(this.apiUrl, dto);
  }

  getProductReviews(productId: number): Observable<ReviewDto[]> {
    return this.http.get<ReviewDto[]>(`${this.apiUrl}/product/${productId}`);
  }

  getAdminReviews(search?: string, rating?: number): Observable<PortalReviewDto[]> {
    return this.http.get<PortalReviewDto[]>(`${this.apiUrl}/admin`, { params: this.reviewParams(search, rating) });
  }

  getSellerReviews(search?: string, rating?: number): Observable<PortalReviewDto[]> {
    return this.http.get<PortalReviewDto[]>(`${this.apiUrl}/seller`, { params: this.reviewParams(search, rating) });
  }

  /** Reviews left by the currently logged-in buyer (used by Profile → My Reviews). */
  getMine(): Observable<MyReviewDto[]> {
    return this.http.get<MyReviewDto[]>(`${this.apiUrl}/me`);
  }

  updateReview(id: number, dto: UpdateReviewDto): Observable<ReviewDto> {
    return this.http.put<ReviewDto>(`${this.apiUrl}/${id}`, dto);
  }

  deleteReview(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private reviewParams(search?: string, rating?: number): HttpParams {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (rating) params = params.set('rating', String(rating));
    return params;
  }
}
