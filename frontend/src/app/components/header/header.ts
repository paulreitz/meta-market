import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';

import { UserMenu } from '../user-menu/user-menu';

@Component({
  selector: 'app-header',
  imports: [RouterLink, UserMenu, MatToolbarModule],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class Header {

}
