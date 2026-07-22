import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/models/search_result.dart';
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
    try {
      var filter = <String, dynamic>{};
      if (_statusFilter != null) {
        filter["status"] = _statusi.indexOf(_statusFilter!);
      }
      var data = await _provider.get(filter: filter);
      setState(() {
        result = data;
        isLoading = false;
      });
    } on Exception catch (e) {
      alertBox(context, 'Greška', e.toString());
    }
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
                  isLoading = true;
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
    return Expanded(
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

  Widget _buildActions(Termin e) {
    List<Widget> actions = [];

    if (e.status == "Zakazan") {
      actions.add(_actionButton("Potvrdi", Icons.check, Colors.blue, () async {
        await _run(() => _provider.potvrdi(e.id!));
      }));
      actions.add(_actionButton("Otkaži", Icons.close, Colors.red, () async {
        await _run(() => _provider.otkazi(e.id!));
      }));
    } else if (e.status == "Potvrdjen") {
      actions.add(_actionButton(
          "Odradjen", Icons.done_all, Colors.green, () async {
        await _run(() => _provider.oznaciOdradjen(e.id!));
      }));
      actions.add(_actionButton("Otkaži", Icons.close, Colors.red, () async {
        await _run(() => _provider.otkazi(e.id!));
      }));
      actions.add(_actionButton(
          "Nije se odazvao", Icons.person_off, Colors.orange, () async {
        await _run(() => _provider.oznaciNijeSeOdazvao(e.id!));
      }));
    }

    if (actions.isEmpty) {
      return Text("-");
    }

    return Wrap(spacing: 4, children: actions);
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
