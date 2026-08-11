import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared-module';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Profile } from './profile';
import { MaterialDropdownModule } from '../../shared/material/material-dropdown.module';

const routes: Routes = [{ path: '', component: Profile }];

@NgModule({
  declarations: [Profile],
  imports: [
    SharedModule,
    ReactiveFormsModule,
    FormsModule,
    MaterialDropdownModule,
    RouterModule.forChild(routes)
  ]
})
export class ProfileModule {}
