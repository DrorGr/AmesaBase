import { Injectable, signal } from '@angular/core';
import { House } from '../models/house.model';

@Injectable({
  providedIn: 'root'
})
export class LotteryService {
  private houses = signal<House[]>([
    {
      id: '1',
      title: 'Modern Downtown Condo',
      description: 'Stunning 2-bedroom condo in the heart of downtown with city views and modern amenities.',
      price: 450000,
      location: 'Downtown, City Center',
      imageUrl: 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&h=600&fit=crop',
      images: [
        { url: 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&h=600&fit=crop', alt: 'Modern downtown condo exterior' },
        { url: 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&h=600&fit=crop', alt: 'Modern downtown condo living room' },
        { url: 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&h=600&fit=crop', alt: 'Modern downtown condo kitchen' }
      ],
      bedrooms: 2,
      bathrooms: 2,
      sqft: 1200,
      lotteryEndDate: new Date('2025-02-15'),
      totalTickets: 1000,
      soldTickets: 650,
      ticketPrice: 50,
      status: 'active'
    },
    {
      id: '2',
      title: 'Suburban Family Home',
      description: 'Beautiful 4-bedroom family home with large backyard and garage in quiet neighborhood.',
      price: 680000,
      location: 'Maple Heights Suburb',
      imageUrl: 'https://images.unsplash.com/photo-1570129477492-45c003edd2be?w=800&h=600&fit=crop',
      images: [
        { url: 'https://images.unsplash.com/photo-1570129477492-45c003edd2be?w=800&h=600&fit=crop', alt: 'Suburban family home exterior' },
        { url: 'https://images.unsplash.com/photo-1570129477492-45c003edd2be?w=800&h=600&fit=crop', alt: 'Suburban family home backyard' },
        { url: 'https://images.unsplash.com/photo-1570129477492-45c003edd2be?w=800&h=600&fit=crop', alt: 'Suburban family home interior' }
      ],
      bedrooms: 4,
      bathrooms: 3,
      sqft: 2400,
      lotteryEndDate: new Date('2025-02-20'),
      totalTickets: 1500,
      soldTickets: 890,
      ticketPrice: 75,
      status: 'active'
    },
    {
      id: '3',
      title: 'Luxury Waterfront Villa',
      description: 'Exclusive waterfront villa with private beach access and panoramic ocean views.',
      price: 1200000,
      location: 'Oceanfront District',
      imageUrl: 'https://images.unsplash.com/photo-1613490493576-7fde63acd811?w=800&h=600&fit=crop',
      images: [
        { url: 'https://images.unsplash.com/photo-1613490493576-7fde63acd811?w=800&h=600&fit=crop', alt: 'Luxury waterfront villa exterior' },
        { url: 'https://images.unsplash.com/photo-1613490493576-7fde63acd811?w=800&h=600&fit=crop', alt: 'Luxury waterfront villa ocean view' },
        { url: 'https://images.unsplash.com/photo-1613490493576-7fde63acd811?w=800&h=600&fit=crop', alt: 'Luxury waterfront villa pool area' }
      ],
      bedrooms: 5,
      bathrooms: 4,
      sqft: 3500,
      lotteryEndDate: new Date('2025-03-01'),
      totalTickets: 2000,
      soldTickets: 1245,
      ticketPrice: 100,
      status: 'active'
    }
  ]);

  getHouses() {
    return this.houses.asReadonly();
  }

  getHouseById(id: string): House | undefined {
    return this.houses().find(house => house.id === id);
  }

  getActiveHouses(): House[] {
    return this.houses().filter(house => house.status === 'active');
  }

  getUpcomingHouses(): House[] {
    return this.houses().filter(house => house.status === 'upcoming');
  }

  getEndedHouses(): House[] {
    return this.houses().filter(house => house.status === 'ended');
  }

  purchaseTicket(houseId: string): Promise<{ success: boolean; message?: string; remainingTickets?: number }> {
    return new Promise((resolve) => {
      setTimeout(() => {
        const houses = this.houses();
        const houseIndex = houses.findIndex(h => h.id === houseId);
        
        if (houseIndex === -1) {
          resolve({ success: false, message: 'House not found' });
          return;
        }

        const house = houses[houseIndex];
        
        if (house.status !== 'active') {
          resolve({ success: false, message: 'Lottery is not active for this house' });
          return;
        }

        if (house.soldTickets >= house.totalTickets) {
          resolve({ success: false, message: 'No tickets remaining for this house' });
          return;
        }

        const updatedHouses = [...houses];
        updatedHouses[houseIndex] = {
          ...updatedHouses[houseIndex],
          soldTickets: updatedHouses[houseIndex].soldTickets + 1
        };
        this.houses.set(updatedHouses);
        
        const remainingTickets = house.totalTickets - (house.soldTickets + 1);
        resolve({ 
          success: true, 
          message: 'Ticket purchased successfully',
          remainingTickets 
        });
      }, 500);
    });
  }
}