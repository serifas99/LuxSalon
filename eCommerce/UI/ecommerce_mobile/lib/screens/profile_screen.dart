import 'package:ecommerce_mobile/providers/auth_provider.dart';
import 'package:ecommerce_mobile/screens/change_password_screen.dart';
import 'package:ecommerce_mobile/screens/profile_settings_screen.dart';
import 'package:ecommerce_mobile/services/signalr_service.dart';
import 'package:ecommerce_mobile/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../main.dart' hide alertBox;
import '../models/user.dart';
import '../providers/user_provider.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  late UserProvider _userProvider;

  User? user;

  bool isLoading = true;

  @override
  void initState() {
    super.initState();

    _userProvider = context.read<UserProvider>();

    initData();
  }

  Future<void> initData() async {
    try {
      var result = await _userProvider.getById(
        int.tryParse(AuthProvider.accessTokenDecoded?['Id']?.toString() ?? '0') ?? 0,
      );

      setState(() {
        user = result;
        isLoading = false;
      });
    } on Exception catch (e) {
      alertBox(context, 'Greška', e.toString());
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Profil")),
      body: SafeArea(
        child: SingleChildScrollView(
          child: isLoading || user == null
              ? const Padding(
                  padding: EdgeInsets.all(32),
                  child: Center(child: CircularProgressIndicator()),
                )
              : Column(
                  children: [
                    SizedBox(height: 30),
                    _buildProfileInfo(),
                    SizedBox(height: 30),
                    _buildProfileMenu(),
                  ],
                ),
        ),
      ),
    );
  }

  Row _buildProfileInfo() {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Column(
          children: [
            CircleAvatar(
              backgroundImage: user!.profileImageBase64 != null
                  ? ImageFromBase64StringWithoutDimnesions(
                      user!.profileImageBase64!,
                    )
                  : AssetImage("assets/images/no_profile.png"),
              radius: 60,
            ),
            const SizedBox(height: 16),
            Text(
              "${user!.firstName ?? ''} ${user!.lastName ?? ''}",
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            Text(
              user!.username ?? "",
              style: TextStyle(fontSize: 14, color: Colors.grey[600]),
            ),
          ],
        ),
      ],
    );
  }

  Padding _buildProfileMenu() {
    return Padding(
      padding: const EdgeInsets.fromLTRB(26.0, 0, 26.0, 0),
      child: Card(
        elevation: 6,
        child: ListView(
          shrinkWrap: true,
          children: [
            ListTile(
              leading: Icon(Icons.edit_outlined),
              title: Text("Uredi profil"),
              onTap: () async {
                var refresh = await Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => ProfileSettingsScreen(user: user!),
                  ),
                );

                if (refresh == 'reload') {
                  initData();
                }
              },
            ),
            ListTile(
              leading: Icon(Icons.lock_outline),
              title: Text("Promijeni lozinku"),
              onTap: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => ChangePasswordScreen(),
                  ),
                );
              },
            ),
            ListTile(
              leading: Icon(Icons.logout),
              title: Text("Odjava"),
              onTap: () async {
                final leave = await showDialog<bool>(
                  context: context,
                  builder: (ctx) => AlertDialog(
                    title: Text("Odjava"),
                    content: Text("Jeste li sigurni da se želite odjaviti?"),
                    actions: [
                      TextButton(
                        onPressed: () => Navigator.pop(ctx, false),
                        child: Text("Otkaži"),
                      ),
                      TextButton(
                        onPressed: () => Navigator.pop(ctx, true),
                        child: Text("Odjavi se"),
                      ),
                    ],
                  ),
                );
                if (leave != true || !mounted) return;
                await SignalRService.instance.disconnect();
                context.read<AuthProvider>().logout();
                if (!mounted) return;
                Navigator.of(context).pushAndRemoveUntil(
                  MaterialPageRoute(builder: (context) => LoginPage()),
                  (route) => route.isFirst,
                );
              },
            ),
          ],
        ),
      ),
    );
  }
}
