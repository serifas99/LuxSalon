import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/auth_provider.dart';
import '../utils/api_client_exception.dart';
import 'reset_password_screen.dart';

/// Prvi korak "Zaboravljena lozinka" toka: korisnik unosi email, backend
/// (ako nalog postoji) šalje 6-cifreni kod na taj email preko RabbitMQ ->
/// Subscriber -> MailHog. Odgovor je namjerno uvijek isti da se ne otkrije
/// koji emailovi su registrovani - vidi komentar u AccessController-u.
class ForgotPasswordScreen extends StatefulWidget {
  const ForgotPasswordScreen({super.key});

  @override
  State<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends State<ForgotPasswordScreen> {
  final TextEditingController _emailController = TextEditingController();
  bool _busy = false;

  Future _posaljiKod() async {
    final email = _emailController.text.trim();
    if (email.isEmpty) {
      _alertBox("Greška", "Unesite email adresu.");
      return;
    }

    setState(() => _busy = true);
    try {
      final authProvider = context.read<AuthProvider>();
      await authProvider.forgotPassword(email);

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            "Ako nalog sa ovim emailom postoji, kod za reset lozinke je poslan.",
          ),
        ),
      );
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (context) => ResetPasswordScreen(email: email),
        ),
      );
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
      appBar: AppBar(title: const Text("Zaboravljena lozinka")),
      body: SingleChildScrollView(
        child: Padding(
          padding: const EdgeInsets.all(30.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const SizedBox(height: 10),
              const Text(
                "Unesite email adresu sa kojom ste registrovani. "
                "Poslaćemo vam kod za reset lozinke.",
              ),
              const SizedBox(height: 20),
              TextField(
                controller: _emailController,
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(
                  labelText: 'Email',
                  border: OutlineInputBorder(),
                ),
              ),
              const SizedBox(height: 30),
              ElevatedButton(
                onPressed: _busy ? null : _posaljiKod,
                child: _busy
                    ? const SizedBox(
                        height: 18,
                        width: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text("Pošalji kod"),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
