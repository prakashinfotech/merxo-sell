import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared-module';
import { AuthRoutingModule } from './auth-routing-module';
import { Login } from './login/login';
import { Register } from './register/register';
import { AdminLogin } from './admin-login/admin-login';

@NgModule({
  declarations: [Login, Register, AdminLogin],
  imports: [SharedModule, ReactiveFormsModule, AuthRoutingModule]
})
export class AuthModule {}
