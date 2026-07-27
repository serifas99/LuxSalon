import 'dart:convert';

import 'package:ecommerce_mobile/models/obavijest.dart';
import 'package:ecommerce_mobile/models/search_result.dart';
import 'package:ecommerce_mobile/providers/obavijest_provider.dart';
import 'package:ecommerce_mobile/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

/// Salonske obavijesti/vijesti (akcije, promjena radnog vremena i sl.) -
/// razlicito od licnih Notifikacija koje su vezane za pojedinacnog korisnika.
class NovostiScreen extends StatefulWidget {
  const NovostiScreen({super.key});

  @override
  State<NovostiScreen> createState() => _NovostiScreenState();
}

class _NovostiScreenState extends State<NovostiScreen> {
  late ObavijestProvider _obavijestProvider;
  SearchResult<Obavijest>? _obavijesti;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _obavijestProvider = context.read<ObavijestProvider>();
    _ucitaj();
  }

  Future _ucitaj() async {
    try {
      final rezultat = await _obavijestProvider
          .get(filter: {"isActive": true, "pageSize": 100});
      rezultat.items?.sort((a, b) =>
          (b.createdAt ?? DateTime(2000)).compareTo(a.createdAt ?? DateTime(2000)));

      if (!mounted) return;
      setState(() {
        _obavijesti = rezultat;
        _isLoading = false;
      });
    } on Exception catch (e) {
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }

  String _formatDatum(DateTime? d) {
    if (d == null) return '';
    return "${d.day.toString().padLeft(2, '0')}.${d.month.toString().padLeft(2, '0')}.${d.year}.";
  }

  @override
  Widget build(BuildContext context) {
    final obavijesti = _obavijesti?.items ?? [];
    return Scaffold(
      appBar: AppBar(title: const Text("Novosti")),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _ucitaj,
              child: obavijesti.isEmpty
                  ? ListView(
                      children: const [
                        Padding(
                          padding: EdgeInsets.all(32),
                          child: Center(child: Text("Trenutno nema novosti.")),
                        ),
                      ],
                    )
                  : ListView.builder(
                      padding: const EdgeInsets.all(12),
                      itemCount: obavijesti.length,
                      itemBuilder: (context, index) {
                        final o = obavijesti[index];
                        return Card(
                          margin: const EdgeInsets.only(bottom: 12),
                          child: Padding(
                            padding: const EdgeInsets.all(12),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                if (o.slikaBase64 != null &&
                                    o.slikaBase64!.isNotEmpty)
                                  ClipRRect(
                                    borderRadius: BorderRadius.circular(8),
                                    child: Image.memory(
                                      base64Decode(o.slikaBase64!),
                                      height: 140,
                                      width: double.infinity,
                                      fit: BoxFit.cover,
                                    ),
                                  ),
                                if (o.slikaBase64 != null &&
                                    o.slikaBase64!.isNotEmpty)
                                  const SizedBox(height: 8),
                                Text(
                                  o.naslov ?? '',
                                  style: const TextStyle(
                                      fontWeight: FontWeight.bold, fontSize: 16),
                                ),
                                const SizedBox(height: 4),
                                Text(o.tekst ?? ''),
                                const SizedBox(height: 6),
                                Text(
                                  _formatDatum(o.createdAt),
                                  style: TextStyle(
                                      color: Colors.grey[600], fontSize: 12),
                                ),
                              ],
                            ),
                          ),
                        );
                      },
                    ),
            ),
    );
  }
}
