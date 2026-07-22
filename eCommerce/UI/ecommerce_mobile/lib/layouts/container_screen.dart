import 'package:ecommerce_mobile/screens/home_screen.dart';
import 'package:ecommerce_mobile/screens/moji_termini_screen.dart';
import 'package:ecommerce_mobile/screens/notifikacije_screen.dart';
import 'package:ecommerce_mobile/screens/profile_screen.dart';
import 'package:flutter/material.dart';

class ContainerScreen extends StatefulWidget {
  const ContainerScreen({super.key});

  @override
  State<ContainerScreen> createState() => _ContainerScreenState();
}

class _ContainerScreenState extends State<ContainerScreen> {
  int _selectedIndex = 0;

  // Namjerno NE koristimo IndexedStack: on bi zadrzao svaki ekran zauvijek
  // "zivim" u memoriji, pa bi npr. "Moji termini" ucitao podatke samo JEDNOM
  // (kad se app prvi put otvori) i nikad se ne bi osvjezio novim terminima.
  // Ovako se svaki put kad se predje na tab kreira nov ekran -> nov initState -> svjez fetch.
  Widget _trenutniEkran() {
    switch (_selectedIndex) {
      case 1:
        return const MojiTerminiScreen();
      case 2:
        return const NotifikacijeScreen();
      case 3:
        return const ProfileScreen();
      case 0:
      default:
        return const HomeScreen();
    }
  }

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: _trenutniEkran(),
      bottomNavigationBar: BottomNavigationBar(
        type: BottomNavigationBarType.fixed,
        selectedItemColor: Colors.red,
        items: const <BottomNavigationBarItem>[
          BottomNavigationBarItem(icon: Icon(Icons.home), label: 'Početna'),
          BottomNavigationBarItem(icon: Icon(Icons.event), label: 'Termini'),
          BottomNavigationBarItem(
              icon: Icon(Icons.notifications), label: 'Obavještenja'),
          BottomNavigationBarItem(icon: Icon(Icons.person), label: 'Profil'),
        ],
        currentIndex: _selectedIndex,
        onTap: _onItemTapped,
      ),
    );
  }
}
