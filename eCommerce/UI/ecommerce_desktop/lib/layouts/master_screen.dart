import 'package:ecommerce_desktop/screens/product_details_screen.dart';
import 'package:ecommerce_desktop/screens/product_list.dart';
import 'package:ecommerce_desktop/screens/review_list.dart';
import 'package:ecommerce_desktop/screens/user_details_screen.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../main.dart';
import '../providers/auth_provider.dart';
import '../providers/user_provider.dart';
import '../screens/category_list.dart';
import '../screens/klijent_list.dart';
import '../screens/dashboard_screen.dart';
import '../screens/usluga_kategorija_list.dart';
import '../screens/usluga_list.dart';
import '../screens/frizer_list.dart';
import '../screens/termin_list.dart';
import '../screens/izvjestaji_screen.dart';
import '../services/signalr_service.dart';
import '../utils/utils_widgets.dart';

class MasterScreen extends StatefulWidget {
  const MasterScreen({super.key, required this.child, required this.title});
  final Widget child;
  final String title;

  @override
  State<MasterScreen> createState() => _MasterScreenState();
}

class _MasterScreenState extends State<MasterScreen> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.title),
        centerTitle: true,
      ),
      drawer: Drawer(
        child: ListView(
          padding: EdgeInsets.zero,
          children: [
            DrawerHeader(
              decoration: BoxDecoration(
                color: Colors.blue,
              ),
              child: Text(
                'Menu',
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 24,
                ),
              ),
            ),
            ListTile(
              leading: Icon(Icons.dashboard),
              title: Text('Pregled'),
              onTap: () {
                Navigator.push(context, MaterialPageRoute(builder: (context)=> DashboardScreen()));
              },
            ),
            ListTile(
              leading: Icon(Icons.event),
              title: Text('Termini'),
              onTap: () {
                Navigator.push(context, MaterialPageRoute(builder: (context)=> TerminList()));
              },
            ),
            ListTile(
              leading: Icon(Icons.content_cut),
              title: Text('Usluge'),
              onTap: () {
                Navigator.push(context, MaterialPageRoute(builder: (context)=> UslugaList()));
              },
            ),
            ListTile(
              leading: Icon(Icons.category),
              title: Text('Kategorije usluga'),
              onTap: () {
                Navigator.push(context, MaterialPageRoute(builder: (context)=> UslugaKategorijaList()));
              },
            ),
            ListTile(
              leading: Icon(Icons.badge_outlined),
              title: Text('Zaposleni (frizeri)'),
              onTap: () {
                Navigator.push(context, MaterialPageRoute(builder: (context)=> FrizerList()));
              },
            ),
            ListTile(
              leading: Icon(Icons.people),
              title: Text('Klijenti'),
              onTap: () {
                Navigator.push(context, MaterialPageRoute(builder: (context) => KlijentList()));
              },
            ),
            ListTile(
              leading: Icon(Icons.picture_as_pdf),
              title: Text('Izvještaji'),
              onTap: () {
                Navigator.push(context, MaterialPageRoute(builder: (context)=> IzvjestajiScreen()));
              },
            ),
            ListTile(
              leading: Icon(Icons.account_circle),
              title: Text('Moj profil'),
              onTap: () async {
                Navigator.pop(context); // zatvori drawer prije async poziva
                try {
                  final userProvider = context.read<UserProvider>();
                  final mojId = int.tryParse(
                      AuthProvider.accessTokenDecoded?['Id']?.toString() ?? '');
                  if (mojId == null) return;
                  final ja = await userProvider.getById(mojId);
                  if (!context.mounted) return;
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) =>
                          UserDetailsScreen(user: ja, hideStatus: true),
                    ),
                  );
                } catch (e) {
                  if (context.mounted) {
                    alertBox(context, "Greška", e.toString());
                  }
                }
              },
            ),
            ListTile(
              leading: Icon(Icons.logout),
              title: Text('Logout'),
              onTap: () {
                  showDialog(
                    context: context, builder: (BuildContext context) => AlertDialog(
                      title: Text("Logout"),
                      content: Text("Are you sure you want to logout?"),
                      actions: [
                        TextButton(
                          onPressed: (() {
                            Navigator.pop(context);
                          }),
                          child: Text("Cancel"),
                        ),
                        TextButton(
                          onPressed: () async {
                            try {
                              AuthProvider authProvider = context
                                  .read<AuthProvider>();

                              SignalRService.instance.disconnect();
                              authProvider.logout();

                              //throw Exception("Logout successful");

                              Navigator.pushAndRemoveUntil(
                                context,
                                MaterialPageRoute(builder: (_) => LoginScreen()),
                                (route) => false,
                              );
                            } catch (e) {
                              alertBoxMoveBack(context, "Error", e.toString());
                            }
                          },
                          child: Text("Yes"),
                        ),
                      ],
                    ));
              },
            ),
          ],
        ),
      ),
      body: widget.child,
      );
  }
}