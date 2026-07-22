import 'package:ecommerce_mobile/models/placanje.dart';
import 'package:ecommerce_mobile/models/termin.dart';
import 'package:ecommerce_mobile/providers/placanje_provider.dart';
import 'package:ecommerce_mobile/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';

/// Tok placanja:
/// 1. Kreiraj narudzbu na backendu (Placanje/Kreiraj/{terminId}) -> dobijemo link ka PayPal sandboxu.
/// 2. Otvorimo taj link u browseru gdje se korisnik uloguje na PayPal (sandbox) nalog i odobri placanje.
/// 3. Korisnik se vrati u aplikaciju i potvrdi da je odobrio - mi tada zovemo
///    Placanje/Potvrdi/{paypalOrderId} koje kaptira novac i mijenja status termina.
class PlacanjeScreen extends StatefulWidget {
  final Termin termin;

  const PlacanjeScreen({super.key, required this.termin});

  @override
  State<PlacanjeScreen> createState() => _PlacanjeScreenState();
}

class _PlacanjeScreenState extends State<PlacanjeScreen> {
  late PlacanjeProvider _placanjeProvider;

  bool _kreiranje = false;
  bool _potvrdjivanje = false;
  String? _paypalOrderId;
  String? _approvalUrl;
  Placanje? _rezultat;

  @override
  void initState() {
    super.initState();
    _placanjeProvider = context.read<PlacanjeProvider>();
  }

  Future _kreirajNarudzbu() async {
    setState(() => _kreiranje = true);
    try {
      final rezultat = await _placanjeProvider.kreiraj(widget.termin.id!);
      setState(() {
        _paypalOrderId = rezultat.paypalOrderId;
        _approvalUrl = rezultat.approvalUrl;
      });

      if (_approvalUrl != null) {
        await launchUrl(Uri.parse(_approvalUrl!),
            mode: LaunchMode.externalApplication);
      }
    } on Exception catch (e) {
      if (mounted) alertBox(context, "Greška", e.toString());
    } finally {
      if (mounted) setState(() => _kreiranje = false);
    }
  }

  Future _otvoriPonovo() async {
    if (_approvalUrl == null) return;
    await launchUrl(Uri.parse(_approvalUrl!), mode: LaunchMode.externalApplication);
  }

  Future _potvrdiPlacanje() async {
    if (_paypalOrderId == null) return;
    setState(() => _potvrdjivanje = true);
    try {
      final rezultat = await _placanjeProvider.potvrdi(_paypalOrderId!);
      setState(() => _rezultat = rezultat);

      if (rezultat.status == "Zavrseno") {
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Plaćanje uspješno završeno!")),
        );
        Navigator.pop(context, "reload");
      }
    } on Exception catch (e) {
      if (mounted) alertBox(context, "Greška", e.toString());
    } finally {
      if (mounted) setState(() => _potvrdjivanje = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final t = widget.termin;
    return Scaffold(
      appBar: AppBar(title: const Text("Plaćanje")),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Card(
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12)),
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(t.uslugaNaziv ?? '',
                        style: const TextStyle(
                            fontSize: 17, fontWeight: FontWeight.bold)),
                    Text("Frizer: ${t.frizerImePrezime ?? ''}"),
                    const SizedBox(height: 8),
                    Text("Iznos za platiti: ${t.cijena ?? 0} KM (PayPal - USD)",
                        style: const TextStyle(fontWeight: FontWeight.bold)),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 24),
            if (_approvalUrl == null) ...[
              const Text(
                "Kliknite na dugme ispod da otvorite PayPal (sandbox) stranicu za odobravanje plaćanja.",
              ),
              const SizedBox(height: 16),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  onPressed: _kreiranje ? null : _kreirajNarudzbu,
                  icon: const Icon(Icons.open_in_browser),
                  label: _kreiranje
                      ? const SizedBox(
                          height: 18,
                          width: 18,
                          child: CircularProgressIndicator(strokeWidth: 2))
                      : const Text("Plati sa PayPal"),
                ),
              ),
            ] else ...[
              const Text(
                "1. Odobrite plaćanje u browseru koji se otvorio (prijavite se na PayPal sandbox nalog).\n"
                "2. Vratite se ovdje i pritisnite \"Potvrdi plaćanje\".",
              ),
              const SizedBox(height: 16),
              SizedBox(
                width: double.infinity,
                child: OutlinedButton.icon(
                  onPressed: _otvoriPonovo,
                  icon: const Icon(Icons.open_in_new),
                  label: const Text("Otvori PayPal ponovo"),
                ),
              ),
              const SizedBox(height: 12),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  onPressed: _potvrdjivanje ? null : _potvrdiPlacanje,
                  icon: const Icon(Icons.check_circle_outline),
                  label: _potvrdjivanje
                      ? const SizedBox(
                          height: 18,
                          width: 18,
                          child: CircularProgressIndicator(strokeWidth: 2))
                      : const Text("Potvrdi plaćanje"),
                ),
              ),
              if (_rezultat != null && _rezultat!.status != "Zavrseno") ...[
                const SizedBox(height: 12),
                Text(
                  "Status plaćanja: ${_rezultat!.status}",
                  style: const TextStyle(color: Colors.orange),
                ),
              ],
            ],
          ],
        ),
      ),
    );
  }
}
