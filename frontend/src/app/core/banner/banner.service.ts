import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';

// ── Public model ────────────────────────────────────────────────────────────
export interface OfferBannerDto {
  bannerId: number;
  slot: string;
  title: string;
  subtitle?: string;
  badgeText?: string;
  imageUrl: string;
  sideImageUrl?: string;
  ctaLabel?: string;
  ctaUrl?: string;
  secondaryLabel?: string;
  secondaryUrl?: string;
  backgroundColor?: string;
  textColor?: string;
  linkedProductId?: number;
  linkedProductName?: string;
  linkedCategoryId?: number;
  linkedCategoryName?: string;
  startsAt?: string;
  endsAt?: string;
  sortOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface BannersBySlot {
  hero: OfferBannerDto[];
  midLeft: OfferBannerDto[];
  midRight: OfferBannerDto[];
  strip: OfferBannerDto[];
}

// ── Admin form models ────────────────────────────────────────────────────────
export interface CreateOfferBannerForm {
  slot: string;
  title: string;
  subtitle?: string;
  badgeText?: string;
  imageUrl: string;
  sideImageUrl?: string;
  ctaLabel?: string;
  ctaUrl?: string;
  secondaryLabel?: string;
  secondaryUrl?: string;
  backgroundColor?: string;
  textColor?: string;
  linkedProductId?: number | null;
  linkedCategoryId?: number | null;
  startsAt?: string | null;
  endsAt?: string | null;
  sortOrder: number;
  isActive: boolean;
}

export type UpdateOfferBannerForm = CreateOfferBannerForm;

export const OFFER_BANNER_SLOTS = ['Hero', 'MidLeft', 'MidRight', 'Strip'] as const;
export type OfferBannerSlot = typeof OFFER_BANNER_SLOTS[number];

// ── Service ─────────────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class OfferBannerService {
  private readonly publicBase = `${environment.apiUrl}/offer-banners`;
  private readonly adminBase  = `${environment.apiUrl}/admin/offer-banners`;
  private readonly mediaBase  = `${environment.apiUrl}/media`;

  private readonly _bannersSubject = new BehaviorSubject<BannersBySlot | null>(null);
  readonly banners$ = this._bannersSubject.asObservable();

  constructor(private http: HttpClient) {}

  // ── Public ─────────────────────────────────────────────────────────────────

  /** Fetches active banners grouped by slot and caches in BehaviorSubject. */
  loadPublicBanners(): Observable<BannersBySlot> {
    return this.http
      .get<ApiResponse<BannersBySlot>>(this.publicBase)
      .pipe(
        map(r => r.data),
        tap(banners => this._bannersSubject.next(banners)),
      );
  }

  get cachedBanners(): BannersBySlot | null {
    return this._bannersSubject.getValue();
  }

  // ── Admin ───────────────────────────────────────────────────────────────────

  getAll(): Observable<OfferBannerDto[]> {
    return this.http
      .get<ApiResponse<OfferBannerDto[]>>(this.adminBase)
      .pipe(map(r => r.data));
  }

  getById(id: number): Observable<OfferBannerDto> {
    return this.http
      .get<ApiResponse<OfferBannerDto>>(`${this.adminBase}/${id}`)
      .pipe(map(r => r.data));
  }

  create(form: CreateOfferBannerForm): Observable<OfferBannerDto> {
    return this.http
      .post<ApiResponse<OfferBannerDto>>(this.adminBase, form)
      .pipe(map(r => r.data));
  }

  update(id: number, form: UpdateOfferBannerForm): Observable<OfferBannerDto> {
    return this.http
      .put<ApiResponse<OfferBannerDto>>(`${this.adminBase}/${id}`, form)
      .pipe(map(r => r.data));
  }

  setActive(id: number, isActive: boolean): Observable<{ isActive: boolean }> {
    return this.http
      .patch<ApiResponse<{ isActive: boolean }>>(`${this.adminBase}/${id}/active`, { isActive })
      .pipe(map(r => r.data));
  }

  delete(id: number): Observable<unknown> {
    return this.http.delete(`${this.adminBase}/${id}`);
  }

  /** Upload image via the shared MediaController. Returns the URL string. */
  uploadImage(file: File): Observable<string> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http
      .post<{ url: string }>(`${this.mediaBase}/upload`, fd)
      .pipe(map(r => r.url));
  }
}
