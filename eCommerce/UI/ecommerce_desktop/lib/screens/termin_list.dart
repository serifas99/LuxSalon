import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/models/search_result.dart';
import 'package:ecommerce_desktop/providers/placanje_provider.dart';
import 'package:ecommerce_desktop/providers/termin_provider.dart';
import 'package:ecommerce_desktop/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/termin.dart';

class TerminList extends StatefulWidget {
  const TerminList({super.key});

  @override
  State<TerminList> createState() => _TerminListState();
}

class _TerminListState extends State<TerminList> {
  late TerminProvider _provider;
  SearchResult<Termin>? result;
  bool isLoading = true;

  String? _statusFilter;
  int _page = 1;
  static const int _pageSize = 10;

  final List<String> _statusi = [
    "Zakazan",
    "Potvrdjen",
    "Odradjen",
    "Otkazan",
    "NijeSeOdazvao",
  ];

  @override
  void initState() {
    super.initState();
    _provider = context.read<TerminProvider>();
    initTable();
  }

  Future<void> initTable() async {
    setState(() => isLoading = true);
    try {
      var filter = <String, dynamic>{
        "page": _page,
        "pageSize": _pageSize,
        "includeTotalCount": true,
      };
      if (_statusFilter != null) {
        filter["status"] = _statusi.indexOf(_statusFilter!);
      }
      var data = await _provider.get(filter: filter);
      setState(() {
        result = data;
        isLoading = false;
      });
    } on Exception catch (e) {
      setState(() => isLoading = false);
      alertBox(context, 'Greška', e.toString());
    }
  }

  void _idiNaStranicu(int novaStranica) {
    setState(() => _page = novaStranica);
    initTable();
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: "Termini",
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            _buildFilter(),
            isLoading ? CircularProgressIndicator() : _buildTable(),
          ],
        ),
      ),
    );
  }

  Padding _buildFilter() {
    return Padding(
      padding: const EdgeInsets.all(8.0),
      child: Row(
        children: [
          Expanded(
            child: DropdownButtonFormField<String?>(
              value: _statusFilter,
              decoration: InputDecoration(label: Text("Status")),
              items: [
                const DropdownMenuItem<String?>(
                  value: null,
                  child: Text("Svi statusi"),
                ),
                ..._statusi.map(
                  (s) => DropdownMenuItem(value: s, child: Text(s)),
                ),
              ],
              onChanged: (value) {
                setState(() {
                  _statusFilter = value;
                  _page = 1;
                });
                initTable();
              },
            ),
          ),
        ],
      ),
    );
  }

  Expanded _buildTable() {
    final totalCount = result?.totalCount ?? 0;
    final ukupnoStranica =
        totalCount == 0 ? 1 : ((totalCount - 1) ~/ _pageSize) + 1;
    final prikazanoOd = totalCount == 0 ? 0 : ((_page - 1) * _pageSize) + 1;
    final prikazanoDo = totalCount == 0
        ? 0
        : (prikazanoOd + (result?.items?.length ?? 0) - 1);

    return Expanded(
      child: Column(
        children: [
          Expanded(
            child: SizedBox(
              width: double.infinity,
              child: SingleChildScrollView(
                child: DataTable(
                  columns: [
                    DataColumn(label: Text("Klijent")),
                    DataColumn(label: Text("Frizer")),
                    DataColumn(label: Text("Usluga")),
                    DataColumn(label: Text("Datum i vrijeme")),
                    DataColumn(label: Text("Cijena")),
                    DataColumn(label: Text("Status")),
                    DataColumn(label: Text("Akcije")),
                  ],
                  rows: result?.items
                          ?.map(
                            (e) => DataRow(
                              cells: [
                                DataCell(Text(e.klijentImePrezime ?? '')),
                                DataCell(Text(e.frizerImePrezime ?? '')),
                                DataCell(Text(e.uslugaNaziv ?? '')),
                                DataCell(Text(e.datumVrijeme != null
                                    ? "${e.datumVrijeme!.day.toString().padLeft(2, '0')}.${e.datumVrijeme!.month.toString().padLeft(2, '0')}.${e.datumVrijeme!.year}. ${e.datumVrijeme!.hour.toString().padLeft(2, '0')}:${e.datumVrijeme!.minute.toString().padLeft(2, '0')}"
                                    : '')),
                                DataCell(Text(
                                    e.cijena != null ? "${e.cijena} KM" : '')),
                                DataCell(_statusBadge(e.status)),
                                DataCell(_buildActions(e)),
                              ],
                            ),
                          )
                          .toList() ??
                      List.empty(),
                ),
              ),
            ),
          ),
          const SizedBox(height: 12),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text(totalCount == 0
                  ? "Nema termina"
                  : "Prikazano $prikazanoOd do $prikazanoDo od ukupno $totalCount termina"),
              const SizedBox(width: 16),
              IconButton(
                icon: const Icon(Icons.chevron_left),
                onPressed: _page > 1 ? () => _idiNaStranicu(_page - 1) : null,
              ),
              Text("Stranica $_page od $ukupnoStranica"),
              IconButton(
                icon: const Icon(Icons.chevron_right),
                onPressed: _page < ukupnoStranica
                    ? () => _idiNaStranicu(_page + 1)
                    : null,
              ),
            ],
          ),
          const SizedBox(height: 8),
        ],
      ),
    );
  }

  Widget _statusBadge(String? status) {
    Color color;
    switch (status) {
      case "Potvrdjen":
        color = Colors.blue;
        break;
      case "Odradjen":
        color = Colors.green;
        break;
      case "Otkazan":
      case "NijeSeOdazvao":
        color = Colors.red;
        break;
      default:
        color = Colors.orange;
    }
    return Container(
      padding: EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: color.withOpacity(0.15),
        borderRadius: BorderRadius.circular(6),
      ),
      child: Text(status ?? '', style: TextStyle(color: color)),
    );
  }

  // Akcije u tabeli (po prijavi teme): "Pregled" (detalji termina) i "Otkazivanje"
  // (dostupno samo za termine koji jos nisu zavrseni). Promjena statusa (Potvrdi/Odradjen/
  // Nije se odazvao) je i dalje potrebna funkcionalnost (state machine), ali je premjestena
  // u dijalog "Pregled" da bi sama tabela vizuelno odgovarala opisu iz prijave.
  Widget _buildActions(Termin e) {
    final aktivan = e.status == "Zakazan" || e.status == "Potvrdjen";

    return Wrap(
      spacing: 4,
      children: [
        _actionButton("Pregled", Icons.visibility_outlined, Colors.blueGrey,
            () => _prikaziPregled(e)),
        if (aktivan)
          _actionButton("Otkaži", Icons.close, Colors.red, () async {
            final potvrda = await showDialog<bool>(
              context: context,
              builder: (ctx) => AlertDialog(
                title: const Text("Otkazivanje termina"),
                content: const Text(
                    "Jeste li sigurni da želite otkazati ovaj termin? Ova akcija se ne može poništiti."),
                actions: [
                  TextButton(
                    onPressed: () => Navigator.pop(ctx, false),
                    child: const Text("Ne"),
                  ),
                  ElevatedButton(
                    onPressed: () => Navigator.pop(ctx, true),
                    child: const Text("Da, otkaži"),
                  ),
                ],
              ),
            );
            if (potvrda == true) {
              await _run(() => _provider.otkazi(e.id!));
            }
          }),
      ],
    );
  }

  void _prikaziPregled(Termin e) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text("Detalji termina"),
        content: SizedBox(
          width: 380,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _detaljRed("Klijent", e.klijentImePrezime),
              _detaljRed("Frizer", e.frizerImePrezime),
              _detaljRed("Usluga", e.uslugaNaziv),
              _detaljRed(
                  "Datum i vrijeme",
                  e.datumVrijeme != null
                      ? "${e.datumVrijeme!.day.toString().padLeft(2, '0')}.${e.datumVrijeme!.month.toString().padLeft(2, '0')}.${e.datumVrijeme!.year}. ${e.datumVrijeme!.hour.toString().padLeft(2, '0')}:${e.datumVrijeme!.minute.toString().padLeft(2, '0')}"
                      : null),
              _detaljRed("Trajanje",
                  e.trajanjeMinuta != null ? "${e.trajanjeMinuta} min" : null),
              _detaljRed(
                  "Cijena", e.cijena != null ? "${e.cijena} KM" : null),
              _detaljRed("Status", e.status),
              if ((e.napomena ?? '').isNotEmpty)
                _detaljRed("Napomena", e.napomena),
              const SizedBox(height: 16),
              const Text("Promjena statusa",
                  style: TextStyle(fontWeight: FontWeight.bold)),
              const SizedBox(height: 8),
              Wrap(spacing: 8, runSpacing: 8, children: _statusAkcije(e)),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text("Zatvori"),
          ),
        ],
      ),
    );
  }

  List<Widget> _statusAkcije(Termin e) {
    final akcije = <Widget>[];

    if (e.status == "Zakazan") {
      akcije.add(ElevatedButton.icon(
        icon: const Icon(Icons.check, size: 18),
        label: const Text("Potvrdi"),
        onPressed: () async {
          Navigator.pop(context);
          await _run(() => _provider.potvrdi(e.id!));
        },
      ));
    } else if (e.status == "Potvrdjen") {
      akcije.add(ElevatedButton.icon(
        icon: const Icon(Icons.done_all, size: 18),
        label: const Text("Odrađen"),
        onPressed: () async {
          Navigator.pop(context);
          await _run(() => _provider.oznaciOdradjen(e.id!));
        },
      ));
      akcije.add(OutlinedButton.icon(
        icon: const Icon(Icons.person_off, size: 18),
        label: const Text("Nije se odazvao"),
        onPressed: () async {
          Navigator.pop(context);
          await _run(() => _provider.oznaciNijeSeOdazvao(e.id!));
        },
      ));
    }

    // Backend (PlacanjeService.VratiNovacAsync) dozvoljava refund za bilo koji zavrseno
    // placen termin, ne samo Odradjen - ako termin jos nije Odradjen, refund ga automatski
    // otkazuje. Zato se dugme prikazuje za svaki status dokle god je placanje Zavrseno.
    if (e.placanjeStatus == "Zavrseno") {
      akcije.add(OutlinedButton.icon(
        icon: const Icon(Icons.replay, size: 18, color: Colors.red),
        label: const Text("Vrati novac", style: TextStyle(color: Colors.red)),
        onPressed: () async {
          Navigator.pop(context);
          await _vratiNovac(e);
        },
      ));
    }

    if (akcije.isEmpty) {
      akcije.add(Text("Nema dostupnih promjena statusa.",
          style: TextStyle(color: Colors.grey.shade600)));
    }

    return akcije;
  }

  // Refund je nepovratna akcija (novac se stvarno vraca preko PayPal-a) pa trazi
  // eksplicitnu potvrdu, isto kao otkazivanje termina.
  Future _vratiNovac(Termin e) async {
    if (e.placanjeId == null) return;

    final potvrda = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text("Povrat novca"),
        content: const Text(
            "Jeste li sigurni da želite vratiti novac klijentu za ovaj termin? Ova akcija se ne može poništiti."),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text("Ne"),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text("Da, vrati novac"),
          ),
        ],
      ),
    );
    if (potvrda != true) return;

    final placanjeProvider = context.read<PlacanjeProvider>();
    await _run(() => placanjeProvider.vrati(e.placanjeId!));
  }

  Widget _detaljRed(String naziv, String? vrijednost) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 120,
            child: Text(naziv,
                style: TextStyle(
                    fontWeight: FontWeight.w600,
                    color: Colors.grey.shade700)),
          ),
          Expanded(child: Text(vrijednost ?? '-')),
        ],
      ),
    );
  }

  Widget _actionButton(
      String tooltip, IconData icon, Color color, VoidCallback onPressed) {
    return IconButton(
      icon: Icon(icon, color: color, size: 20),
      tooltip: tooltip,
      onPressed: onPressed,
    );
  }

  Future _run(Future Function() action) async {
    try {
      await action();
      initTable();
    } on Exception catch (e) {
      alertBox(context, "Greška", e.toString());
    }
  }
}
