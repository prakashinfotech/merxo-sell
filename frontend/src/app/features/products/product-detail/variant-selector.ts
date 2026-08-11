import {
  Component, Input, Output, EventEmitter, ChangeDetectionStrategy,
  OnChanges, SimpleChanges
} from '@angular/core';
import { CurrencyService } from '../../../core/currency/currency.service';
import { ProductVariant } from '../../../shared/models/product.models';

/**
 * Two-axis selector: colour swatches + size buttons. Emits the resolved
 * ProductVariant whenever the (color, size) pair lands on an actual row, or
 * null when the selection is partial / matches no active variant.
 */
@Component({
  selector: 'app-variant-selector',
  templateUrl: './variant-selector.html',
  styleUrl: './variant-selector.scss',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VariantSelector implements OnChanges {
  @Input() variants: ProductVariant[] = [];
  @Input() allColors: string[] = [];
  @Input() allSizes:  string[] = [];
  @Output() variantSelect = new EventEmitter<ProductVariant | null>();

  selectedColor: string | null = null;
  selectedSize:  string | null = null;

  constructor(readonly cs: CurrencyService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['variants']) {
      const def = this.variants.find(v => v.isDefault && v.isActive) ?? this.variants.find(v => v.isActive);
      if (def) {
        this.selectedColor = def.color || null;
        this.selectedSize  = def.size  || null;
        this.emit();
      }
    }
  }

  get colors(): string[] {
    if (this.allColors?.length > 0) return this.allColors;
    return [...new Set(this.variants.filter(v => v.isActive && v.color).map(v => v.color!))];
  }

  get sizes(): string[] {
    if (this.allSizes?.length > 0) return this.allSizes;
    return [...new Set(this.variants.filter(v => v.isActive && v.size).map(v => v.size!))];
  }

  /** Active variants matching the current colour (used to enable/disable size buttons). */
  get sizesForColor(): Set<string> {
    if (!this.selectedColor) return new Set(this.sizes);
    return new Set(
      this.variants
        .filter(v => v.isActive && v.color === this.selectedColor && (v.size ?? '') !== '' && v.stock > 0)
        .map(v => v.size!)
    );
  }

  isColorSelected(c: string): boolean { return this.selectedColor === c; }
  isSizeSelected(s: string):  boolean { return this.selectedSize  === s; }

  isSizeAvailable(s: string): boolean {
    // If product has no colour axis, just check stock by size.
    if (this.colors.length === 0) {
      return this.variants.some(v => v.isActive && v.size === s && v.stock > 0);
    }
    return this.sizesForColor.has(s);
  }

  isColorAvailable(c: string): boolean {
    return this.variants.some(v => v.isActive && v.color === c && v.stock > 0);
  }

  pickColor(c: string): void {
    this.selectedColor = this.selectedColor === c ? null : c;
    // If current size is no longer valid for the new colour, drop it.
    if (this.selectedSize && this.selectedColor && !this.sizesForColor.has(this.selectedSize)) {
      this.selectedSize = null;
    }
    this.emit();
  }

  pickSize(s: string): void {
    this.selectedSize = this.selectedSize === s ? null : s;
    this.emit();
  }

  /** Resolve the (color, size) pair to a concrete variant. */
  private resolve(): ProductVariant | null {
    // If the product only has colour OR size, allow single-axis resolution.
    if (this.colors.length === 0 && this.sizes.length === 0) return null;

    const needColor = this.colors.length > 0;
    const needSize  = this.sizes.length  > 0;

    if (needColor && !this.selectedColor) return null;
    if (needSize  && !this.selectedSize)  return null;

    return this.variants.find(v =>
      v.isActive
      && (!needColor || v.color === this.selectedColor)
      && (!needSize  || v.size  === this.selectedSize)
    ) ?? null;
  }

  private emit(): void {
    const resolved = this.resolve();
    if (resolved) {
      this.variantSelect.emit(resolved);
    } else if (this.selectedColor) {
      // Color chosen but size not yet picked — emit the first active variant
      // matching that color so the gallery can filter by color immediately.
      const colorVariant = this.variants.find(v => v.isActive && v.color === this.selectedColor) ?? null;
      this.variantSelect.emit(colorVariant);
    } else {
      this.variantSelect.emit(null);
    }
  }
}
