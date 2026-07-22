import 'package:ecommerce_mobile/models/notifikacija.dart';
import 'package:ecommerce_mobile/models/search_result.dart';
import 'package:ecommerce_mobile/providers/auth_provider.dart';
import 'package:ecommerce_mobile/providers/notifikacija_provider.dart';
import 'package:ecommerce_mobile/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

class NotifikacijeScreen extends StatefulWidget {
  const NotifikacijeScreen({super.key});

  @override
  State<NotifikacijeScreen> createState() => _NotifikacijeScreenState();
}

class _NotifikacijeScreenState extends State<NotifikacijeScreen> {
  late NotifikacijaProvider _notifikacijaProvider;
  SearchResult<Notifikacija>? _notifikacije;
  bool _isLoading = true;

  int get _korisnikId =>
      int.tryParse(AuthProvider.accessTokenDecoded?['Id']?.toString() ?? '') ??
      0;

  @override
  void initState() {
    super.initState();
    _notifikacijaProvider = context.read<NotifikacijaProvider>();
    _ucitaj();
  }

  Future _ucitaj() async {
    try {
      final rezultat = await _notifikacijaProvider.get(filter: {
        "korisnikId": _korisnikId,
        "pageSize": 1000,
      });
      rezultat.items?.sort((a, b) =>
          (b.createdAt ?? DateTime(2000)).compareTo(a.createdAt ?? DateTime(2000)));

      if (!mounted) return;
      setState(() {
        _notifikacije = rezultat;
        _isLoading = false;
      });
    } on Exception catch (e) {
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }

  String _formatDatumVrijeme(DateTime? d) {
    if (d == null) return '';
    return "${d.day.toString().padLeft(2, '0')}.${d.month.toString().padLeft(2, '0')}.${d.year}. ${d.hour.toString().padLeft(2, '0')}:${d.minute.toString().padLeft(2, '0')}";
  }

  @override
  Widget build(BuildContext context) {
    final notifikacije = _notifikacije?.items ?? [];
    return Scaffold(
      appBar: AppBar(title: const Text("Obavještenja")),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _ucitaj,
              child: notifikacije.isEmpty
                  ? ListView(
                      children: const [
                        Padding(
                          padding: EdgeInsets.all(32),
                          child: Center(child: Text("Nemate obavještenja.")),
                        ),
                      ],
                    )
                  : ListView.builder(
                      itemCount: notifikacije.length,
                      itemBuilder: (context, index) {
                        final n = notifikacije[index];
                        final procitano = n.procitano ?? false;
                        return Container(
                          color: procitano
                              ? null
                              : Colors.red.withValues(alpha: 0.05),
                          child: ListTile(
                            leading: Icon(
                              procitano
                                  ? Icons.notifications_none
                                  : Icons.notifications_active,
                              color: procitano ? Colors.grey : Colors.red,
                            ),
                            title: Text(
                              n.naslov ?? '',
                              style: TextStyle(
                                  fontWeight: procitano
                                      ? FontWeight.normal
                                      : FontWeight.bold),
                            ),
                            subtitle: Text(
                                "${n.poruka ?? ''}\n${_formatDatumVrijeme(n.createdAt)}"),
                            isThreeLine: true,
                            onTap: () async {
                              if (!procitano) {
                                try {
                                  await _notifikacijaProvider
                                      .oznaciProcitano(n.id!);
                                  _ucitaj();
                                } on Exception catch (e) {
                                  if (mounted) {
                                    alertBox(context, "Greška", e.toString());
                                  }
                                }
                              }
                            },
                          ),
                        );
                      },
                    ),
            ),
    );
  }
}
