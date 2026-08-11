export interface ProductImage {
  imageId: number;
  variantIds?: number[];
  imageUrl: string;
  altText: string;
  sortOrder: number;
  isPrimary?: boolean;
}

export interface ProductVariant {
  variantId: number;
  color?: string | null;
  size?: string | null;
  sku?: string | null;
  priceDelta: number;
  stock: number;
  isActive: boolean;
  isDefault: boolean;
  imageIds: number[];

  /** @deprecated Kept for older code paths — derived from color/size. */
  variantType?: string;
  /** @deprecated Kept for older code paths — derived from color/size. */
  value?: string;
  /** @deprecated Use priceDelta. */
  priceAdjustment?: number;
}

export interface RatingSummary {
  average: number;
  totalCount: number;
  distribution: Record<number, number>;
}

export interface ProductListItem {
  productId: number;
  name: string;
  slug: string;
  basePrice: number;
  salePrice: number | null;
  primaryImageUrl: string;
  rating: number;
  reviewCount: number;
  totalSold: number;
  isOnSale: boolean;
  isNew: boolean;
  isFeatured: boolean;
  categoryId: number;
  categoryName: string;
  description?: string;
  availableStock?: number;
  inCartCount?: number;
  topComment?: string;
}

export interface ProductDetail extends ProductListItem {
  description: string;
  images: ProductImage[];
  variants: ProductVariant[];
  colors: string[];
  sizes: string[];
  ratingSummary: RatingSummary;
  availableStock: number;
  inCartCount: number;
}

export type ProductSortBy = 'Newest' | 'PriceAsc' | 'PriceDesc' | 'Rating' | 'BestSelling';

export interface ProductFilter {
  categoryId?: number;
  search?: string;
  minPrice?: number;
  maxPrice?: number;
  minRating?: number;
  inStock?: boolean;
  sellerId?: number;
  sortBy?: ProductSortBy;
  page?: number;
  pageSize?: number;
}

export interface ProductSearchFilter {
  q?: string;
  categoryId?: number;
  sellerId?: number;
  minPrice?: number;
  maxPrice?: number;
  inStock?: boolean;
  sortBy?: ProductSortBy;
  page?: number;
  pageSize?: number;
}

export interface SuggestItem {
  type: 'product' | 'category';
  id: number;
  label: string;
  imageUrl: string | null;
  slug: string | null;
}

export interface PaginationMeta {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PaginatedResponse<T> {
  data: T[];
  pagination: PaginationMeta;
}

export interface Category {
  categoryId: number;
  name: string;
  slug: string;
  parentCategoryId?: number | null;
  iconUrl: string | null;
  isFashion?: boolean;
  children: Category[];
}
