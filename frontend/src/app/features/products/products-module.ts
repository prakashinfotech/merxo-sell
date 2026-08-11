import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared-module';
import { Products } from './products';
import { ProductDetail } from './product-detail/product-detail';
import { ImageGallery } from './product-detail/image-gallery';
import { VariantSelector } from './product-detail/variant-selector';

const routes: Routes = [
  { path: '',    component: Products },
  { path: ':id', component: ProductDetail }
];

@NgModule({
  declarations: [Products, ProductDetail, ImageGallery, VariantSelector],
  imports: [SharedModule, FormsModule, RouterModule.forChild(routes)]
})
export class ProductsModule {}
