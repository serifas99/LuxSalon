import 'package:ecommerce_mobile/models/frizer.dart';
import 'package:ecommerce_mobile/models/search_result.dart';
import 'package:ecommerce_mobile/models/usluga.dart';
import 'package:ecommerce_mobile/providers/frizer_ocjena_provider.dart';
import 'package:ecommerce_mobile/providers/frizer_provider.dart';
import 'package:ecommerce_mobile/screens/novi_termin_screen.dart';
import 'package:ecommerce_mobile/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

class UslugaDetailsScreen extends StatefulWidget {
  final Usluga usluga;

  const UslugaDetailsScreen({super.key, required this.usluga});

  @override
  State<UslugaDetailsScreen> createState() => _UslugaDetailsScreenState();
}

class _UslugaDetailsScreenState extends State<UslugaDetailsScreen> {
  late FrizerProvider _frizerProvider;
  SearchResult<Frizer>? _frizeri;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _frizerProvider = context.read<FrizerProvider>();
    _ucitaj();
  }

  Future _ucitaj() async {
    try {
      final frizeri = await _frizerProvider.get(filter: {
        "uslugaId": widget.usluga.id,
        "isActive": true,
        "pageSize": 100,
      });
      if (!mounted) return;
      setState(() {
        _frizeri = frizeri;
        _isLoading = false;
      });
    } on Exception catch (e) {
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }

  @override
  Widget build(BuildContext context) {
    final usluga = widget.usluga;
    return Scaffold(
      appBar: AppBar(title: Text(usluga.naziv ?? 'Usluga')),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : ListView(
              padding: const EdgeInsets.all(16),
              children: [
                Card(
                  elevation: 2,
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12)),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          usluga.naziv ?? '',
                          style: const TextStyle(
                              fontSize: 20, fontWeight: FontWeight.bold),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          usluga.uslugaKategorijaNaziv ?? '',
                          style: TextStyle(color: Colors.grey.shade600),
                        ),
                        const SizedBox(height: 12),
                        Text(usluga.opis ?? ''),
                        const SizedBox(height: 16),
                        Row(
                          children: [
                            Icon(Icons.payments_outlined,
                                size: 18, color: Colors.red.shade300),
                            const SizedBox(width: 6),
                            Text("${usluga.cijena ?? 0} KM",
                                style: const TextStyle(
                                    fontWeight: FontWeight.bold)),
                            const SizedBox(width: 20),
                            Icon(Icons.access_time,
                                size: 18, color: Colors.red.shade300),
                            const SizedBox(width: 6),
                            Text("${usluga.trajanjeMinuta ?? 0} min"),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 20),
                const Text(
                  "Frizeri koji izvode ovu uslugu",
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 8),
                if ((_frizeri?.items ?? []).isEmpty)
                  const Padding(
                    padding: EdgeInsets.symmetric(vertical: 12),
                    child: Text("Trenutno nema dostupnih frizera za ovu uslugu."),
                  ),
                ...?_frizeri?.items?.map(
                  (f) => Card(
                    margin: const EdgeInsets.only(bottom: 8),
                    child: ListTile(
                      leading: CircleAvatar(
                        backgroundColor: Colors.red.shade50,
                        backgroundImage: (f.profileImageBase64 != null &&
                                f.profileImageBase64!.isNotEmpty)
                            ? ImageFromBase64StringWithoutDimnesions(
                                f.profileImageBase64!)
                            : null,
                        child: (f.profileImageBase64 != null &&
                                f.profileImageBase64!.isNotEmpty)
                            ? null
                            : Icon(Icons.person, color: Colors.red.shade300),
                      ),
                      title: Text(f.imePrezime ?? ''),
                      subtitle: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Text(f.specijalizacija ?? ''),
                          const SizedBox(height: 2),
                          _ProsjecnaOcjenaBadge(frizerId: f.id),
                        ],
                      ),
                      trailing: ElevatedButton(
                        onPressed: () => _zakazi(f),
                        child: const Text("Zakaži"),
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: 12),
                if ((_frizeri?.items ?? []).isNotEmpty)
                  SizedBox(
                    width: double.infinity,
                    child: OutlinedButton(
                      onPressed: () => _zakazi(null),
                      child: const Text("Zakaži termin (izaberi frizera kasnije)"),
                    ),
                  ),
              ],
            ),
    );
  }

  void _zakazi(Frizer? frizer) async {
    final refresh = await Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => NoviTerminScreen(
          usluga: widget.usluga,
          frizeri: _frizeri?.items ?? [],
          odabraniFrizer: frizer,
        ),
      ),
    );
    if (refresh == "reload" && mounted) {
      Navigator.pop(context, "reload");
    }
  }
}

/// Prikazuje prosjecnu ocjenu frizera (na osnovu FrizerOcjena unosa klijenata)
/// pored svakog frizera na listi - GET FrizerOcjena/ProsjecnaOcjena/{frizerId}.
class _ProsjecnaOcjenaBadge extends StatelessWidget {
  final int? frizerId;

  const _ProsjecnaOcjenaBadge({required this.frizerId});

  @override
  Widget build(BuildContext context) {
    if (frizerId == null) return const SizedBox.shrink();

    return FutureBuilder<double>(
      future: context.read<FrizerOcjenaProvider>().prosjecnaOcjena(frizerId!),
      builder: (context, snapshot) {
        if (!snapshot.hasData || snapshot.data == 0) {
          return const SizedBox.shrink();
        }
        return Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.star, size: 14, color: Colors.amber.shade700),
            const SizedBox(width: 3),
            Text(
              snapshot.data!.toStringAsFixed(1),
              style: TextStyle(fontSize: 12, color: Colors.grey.shade700),
            ),
          ],
        );
      },
    );
  }
}
