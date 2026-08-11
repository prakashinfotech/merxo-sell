import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { Category } from '../../../shared/models/product.models';

@Component({
  selector: 'app-category-scroll',
  templateUrl: './category-scroll.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CategoryScroll {
  @Input() categories: Category[] = [];
  @Output() categorySelect = new EventEmitter<number | null>();

  selectedId: number | null = null;

  select(id: number | null): void {
    this.selectedId = id;
    this.categorySelect.emit(id);
  }
}
