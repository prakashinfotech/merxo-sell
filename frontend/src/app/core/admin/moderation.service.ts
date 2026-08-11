import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PendingProductDto {
  productId: number;
  name: string;
  slug: string;
  basePrice: number;
  salePrice?: number | null;
  stock: number;
  status: string;
  sellerStoreName: string;
  sellerEmail: string;
  categoryName: string;
  primaryImageUrl: string | null;
  createdAt: string;
}

export interface FlaggedReviewDto {
  reviewId: number;
  productId: number;
  productName: string;
  productImageUrl: string | null;
  userId: number;
  authorName: string;
  authorEmail: string;
  rating: number;
  comment: string | null;
  flagReason: string | null;
  createdAt: string;
}

/**
 * Wraps the spec-aligned admin moderation routes
 * (api/admin/products/* and api/admin/reviews/*).
 */
@Injectable({ providedIn: 'root' })
export class ModerationService {
  private base = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  // Product moderation
  getPendingProducts(): Observable<PendingProductDto[]> {
    return this.http.get<PendingProductDto[]>(`${this.base}/products/pending`);
  }

  approveProduct(id: number, note?: string): Observable<unknown> {
    return this.http.post(`${this.base}/products/${id}/approve`, { note: note ?? null });
  }

  rejectProduct(id: number, reason: string): Observable<unknown> {
    return this.http.post(`${this.base}/products/${id}/reject`, { reason });
  }

  // Review moderation
  getFlaggedReviews(): Observable<FlaggedReviewDto[]> {
    return this.http.get<FlaggedReviewDto[]>(`${this.base}/reviews/flagged`);
  }

  approveReview(id: number): Observable<unknown> {
    return this.http.post(`${this.base}/reviews/${id}/approve`, {});
  }

  rejectReview(id: number, reason: string): Observable<unknown> {
    return this.http.post(`${this.base}/reviews/${id}/reject`, { reason });
  }
}
