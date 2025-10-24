import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './navbar/navbar.component';
import { ChatComponent } from "./chat/chat.component";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, ChatComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  standalone: true
})
export class AppComponent {
  title = 'chatApp';
}
