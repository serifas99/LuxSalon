import 'package:ecommerce_mobile/models/search_result.dart';
import 'package:ecommerce_mobile/models/termin.dart';
import 'package:ecommerce_mobile/providers/auth_provider.dart';
import 'package:ecommerce_mobile/providers/frizer_ocjena_provider.dart';
import 'package:ecommerce_mobile/providers/termin_provider.dart';
import 'package:ecommerce_mobile/screens/placanje_screen.dart';
import 'package:ecommerce_mobile/utils/api_client_exception.dart';
import 'package:ecommerce_mobile/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

class MojiTerminiScreen extends StatefulWidget {
  const MojiTerminiScreen({super.key});

  @override
  State<MojiTerminiScreen> createState() => _MojiTerminiScreenState();
}

class _MojiTerminiScreenState extends State<MojiTerminiScreen> {
  late TerminProvider _terminProvider;
  SearchResult<Termin>? _termini;
  bool _isLoading = true;

  int get _klijentId =>
      int.tryParse(AuthProvider.accessTokenDecoded?['Id']?.toString() ?? '') ??
      0;

  @override
  void initState() {
    super.initState();
    _terminProvider = context.read<TerminProvider>();
    _ucitaj();
  }

  Future _ucitaj() async {
    try {
      final rezultat = await _terminProvider.get(filter: {
        "klijentId": _klijentId,
        "pageSize": 100,
      });

      rezultat.items?.sort((a, b) =>
          (b.datumVrijeme ?? DateTime(2000)).compareTo(a.datumVrijeme ?? DateTime(2000)));

      if (!mounted) return;
      setState(() {
        _termini = rezultat;
        _isLoading = false;
      });
    } on Exception catch (e) {
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }

  String _formatDatumVrijeme(DateTime? d) {
    if (d == null) return '';
    return "${d.day.toString().padLeft(2, '0')}.${d.month.toString().padLeft(2, '0')}.${d.year}. u ${d.hour.toString().padLeft(2, '0')}:${d.minute.toString().padLeft(2, '0')}";
  }

  Color _statusBoja(String? status) {
    switch (status) {
      case "Potvrdjen":
        return Colors.blue;
      case "Odradjen":
        return Colors.green;
      case "Otkazan":
      case "NijeSeOdazvao":
        return Colors.red;
      default:
        return Colors.orange;
    }
  }

  String _statusTekst(String? status) {
    switch (status) {
      case "Zakazan":
        return "Zakazan";
      case "Potvrdjen":
        return "Potvrđen";
      case "Odradjen":
        return "Odrađen";
      case "Otkazan":
        return "Otkazan";
      case "NijeSeOdazvao":
        return "Niste se odazvali";
      default:
        return status ?? '';
    }
  }

  @override
  Widget build(BuildContext context) {
    final termini = _termini?.items ?? [];
    return Scaffold(
      appBar: AppBar(title: const Text("Moji termini")),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _ucitaj,
              child: termini.isEmpty
                  ? ListView(
                      children: const [
                        Padding(
                          padding: EdgeInsets.all(32),
                          child: Center(
                            child: Text(
                              "Nemate zakazanih termina. Zakažite jedan sa Početne stranice!",
                              textAlign: TextAlign.center,
                            ),
                          ),
                        ),
                      ],
                    )
                  : ListView.builder(
                      padding: const EdgeInsets.all(12),
                      itemCount: termini.length,
                      itemBuilder: (context, index) {
                        final t = termini[index];
                        return Card(
                          margin: const EdgeInsets.only(bottom: 10),
                          shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12)),
                          child: Padding(
                            padding: const EdgeInsets.all(14),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Row(
                                  mainAxisAlignment:
                                      MainAxisAlignment.spaceBetween,
                                  children: [
                                    Expanded(
                                      child: Text(
                                        t.uslugaNaziv ?? '',
                                        style: const TextStyle(
                                            fontWeight: FontWeight.bold,
                                            fontSize: 16),
                                      ),
                                    ),
                                    Container(
                                      padding: const EdgeInsets.symmetric(
                                          horizontal: 8, vertical: 4),
                                      decoration: BoxDecoration(
                                        color: _statusBoja(t.status)
                                            .withValues(alpha: 0.12),
                                        borderRadius:
                                            BorderRadius.circular(6),
                                      ),
                                      child: Text(
                                        _statusTekst(t.status),
                                        style: TextStyle(
                                            color: _statusBoja(t.status),
                                            fontWeight: FontWeight.w600,
                                            fontSize: 12),
                                      ),
                                    ),
                                  ],
                                ),
                                const SizedBox(height: 6),
                                Text("Frizer: ${t.frizerImePrezime ?? ''}"),
                                Text(_formatDatumVrijeme(t.datumVrijeme)),
                                Text("${t.cijena ?? 0} KM",
                                    style: const TextStyle(
                                        fontWeight: FontWeight.bold)),
                                if (t.status == "Zakazan" ||
                                    (t.status == "Potvrdjen" &&
                                        t.placanjeStatus != "Zavrseno")) ...[
                                  const SizedBox(height: 10),
                                  Row(
                                    children: [
                                      if (t.status == "Zakazan")
                                        OutlinedButton(
                                          onPressed: () => _otkazi(t),
                                          style: OutlinedButton.styleFrom(
                                              foregroundColor: Colors.red),
                                          child: const Text("Otkaži"),
                                        ),
                                      if (t.placanjeStatus != "Zavrseno") ...[
                                        const SizedBox(width: 8),
                                        ElevatedButton.icon(
                                          onPressed: () async {
                                            final refresh = await Navigator.push(
                                              context,
                                              MaterialPageRoute(
                                                builder: (context) =>
                                                    PlacanjeScreen(termin: t),
                                              ),
                                            );
                                            if (refresh == "reload") _ucitaj();
                                          },
                                          icon: const Icon(Icons.payment, size: 18),
                                          label: const Text("Plati"),
                                        ),
                                      ],
                                    ],
                                  ),
                                ],
                                if (t.status == "Odradjen") ...[
                                  const SizedBox(height: 10),
                                  OutlinedButton.icon(
                                    onPressed: () => _ocijeni(t),
                                    icon: const Icon(Icons.star_rate, size: 18),
                                    label: const Text("Ocijeni frizera"),
                                  ),
                                ],
                              ],
                            ),
                          ),
                        );
                      },
                    ),
            ),
    );
  }

  Future _otkazi(Termin t) async {
    final potvrda = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text("Otkazivanje termina"),
        content: const Text("Jeste li sigurni da želite otkazati ovaj termin?"),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text("Ne"),
          ),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text("Da, otkaži"),
          ),
        ],
      ),
    );
    if (potvrda != true) return;

    try {
      await _terminProvider.otkazi(t.id!);
      _ucitaj();
    } on Exception catch (e) {
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }

  Future _ocijeni(Termin t) async {
    int odabranaOcjena = 5;
    final komentarController = TextEditingController();

    final poslati = await showDialog<bool>(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setDialogState) => AlertDialog(
          title: Text("Ocijenite frizera ${t.frizerImePrezime ?? ''}"),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: List.generate(5, (i) {
                  final zvijezda = i + 1;
                  return IconButton(
                    onPressed: () =>
                        setDialogState(() => odabranaOcjena = zvijezda),
                    icon: Icon(
                      zvijezda <= odabranaOcjena
                          ? Icons.star
                          : Icons.star_border,
                      color: Colors.amber,
                    ),
                  );
                }),
              ),
              TextField(
                controller: komentarController,
                maxLength: 500,
                maxLines: 3,
                decoration: const InputDecoration(
                  labelText: "Komentar (opciono)",
                  border: OutlineInputBorder(),
                ),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(ctx, false),
              child: const Text("Otkaži"),
            ),
            ElevatedButton(
              onPressed: () => Navigator.pop(ctx, true),
              child: const Text("Pošalji ocjenu"),
            ),
          ],
        ),
      ),
    );

    if (poslati != true) return;

    try {
      final ocjenaProvider = context.read<FrizerOcjenaProvider>();
      await ocjenaProvider.insert({
        "terminId": t.id,
        "ocjena": odabranaOcjena,
        "komentar": komentarController.text.trim().isEmpty
            ? null
            : komentarController.text.trim(),
      });

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Hvala na ocjeni!")),
      );
    } on ApiClientException catch (e) {
      if (mounted) alertBox(context, "Greška", e.message);
    } on Exception catch (e) {
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }
}
