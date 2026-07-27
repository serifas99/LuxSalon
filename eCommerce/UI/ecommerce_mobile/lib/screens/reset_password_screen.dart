import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/auth_provider.dart';
import '../utils/api_client_exception.dart';

/// Drugi korak "Zaboravljena lozinka" toka: korisnik unosi kod dobijen na
/// email i novu lozinku. Backend provjerava kod preko ICryptoService.Verify
/// (isti mehanizam heširanja kao za lozinke) i da nije istekao/iskorišten.
class ResetPasswordScreen extends StatefulWidget {
  final String email;

  const ResetPasswordScreen({super.key, required this.email});

  @override
  State<ResetPasswordScreen> createState() => _ResetPasswordScreenState();
}

class _ResetPasswordScreenState extends State<ResetPasswordScreen> {
  final TextEditingController _codeController = TextEditingController();
  final TextEditingController _newPasswordController = TextEditingController();
  final TextEditingController _confirmPasswordController = TextEditingController();
  bool _busy = false;

  Future _resetuj() async {
    final code = _codeController.text.trim();
    final newPassword = _newPasswordController.text;
    final confirmPassword = _confirmPasswordController.text;

    if (code.isEmpty || newPassword.isEmpty || confirmPassword.isEmpty) {
      _alertBox("Greška", "Popunite sva polja.");
      return;
    }
    if (newPassword != confirmPassword) {
      _alertBox("Greška", "Nova lozinka i potvrda lozinke se ne podudaraju.");
      return;
    }

    setState(() => _busy = true);
    try {
      final authProvider = context.read<AuthProvider>();
      await authProvider.resetPassword(
        widget.email,
        code,
        newPassword,
        confirmPassword,
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Lozinka je uspješno promijenjena. Prijavite se.")),
      );
      Navigator.popUntil(context, (route) => route.isFirst);
    } on ApiClientException catch (e) {
      _alertBox("Greška", e.message);
    } on Exception catch (e) {
      _alertBox("Greška", e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  void _alertBox(String title, String content) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(title),
        content: Text(content),
        actions: [
          ElevatedButton(
            onPressed: () => Navigator.pop(context),
            child: const Text("OK"),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Reset lozinke")),
      body: SingleChildScrollView(
        child: Padding(
          padding: const EdgeInsets.all(30.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 10),
              Text("Kod je poslan na: ${widget.email}"),
              const SizedBox(height: 20),
              TextField(
                controller: _codeController,
                keyboardType: TextInputType.number,
                maxLength: 6,
                decoration: const InputDecoration(
                  labelText: 'Kod (6 cifara)',
                  border: OutlineInputBorder(),
                ),
              ),
              TextField(
                controller: _newPasswordController,
                obscureText: true,
                decoration: const InputDecoration(
                  labelText: 'Nova lozinka',
                  border: OutlineInputBorder(),
                ),
              ),
              const SizedBox(height: 10),
              TextField(
                controller: _confirmPasswordController,
                obscureText: true,
                decoration: const InputDecoration(
                  labelText: 'Potvrdite novu lozinku',
                  border: OutlineInputBorder(),
                ),
              ),
              const SizedBox(height: 30),
              ElevatedButton(
                onPressed: _busy ? null : _resetuj,
                child: _busy
                    ? const SizedBox(
                        height: 18,
                        width: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text("Resetuj lozinku"),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
