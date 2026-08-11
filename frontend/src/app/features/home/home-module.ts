import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared-module';
import { Home } from './home';
import { CategoryScroll } from './components/category-scroll';

const routes: Routes = [{ path: '', component: Home }];

@NgModule({
  declarations: [Home, CategoryScroll],
  imports: [SharedModule, RouterModule.forChild(routes)]
})
export class HomeModule {}
