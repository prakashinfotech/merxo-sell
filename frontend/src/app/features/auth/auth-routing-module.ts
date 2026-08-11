import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Login } from './login/login';
import { Register } from './register/register';
import { AdminLogin } from './admin-login/admin-login';

const routes: Routes = [
  { path: 'login',        component: Login },       // Buyer
  { path: 'seller/login', component: Login },       // Seller (shares component)
  { path: 'admin/login',  component: AdminLogin },  // SuperAdmin
  { path: 'register',     component: Register },
  { path: '',             redirectTo: 'login', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule {}
