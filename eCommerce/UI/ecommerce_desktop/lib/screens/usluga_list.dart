import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/models/search_result.dart';
import 'package:ecommerce_desktop/providers/usluga_provider.dart';
import 'package:ecommerce_desktop/screens/usluga_details_screen.dart';
import 'package:ecommerce_desktop/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/usluga.dart';

class UslugaList extends StatefulWidget {
  const UslugaList({super.key});

  @override
  State<UslugaList> createState() => _UslugaListState();
}

class _UslugaListState extends State<UslugaList> {
  late UslugaProvider _provider;
  SearchResult<Usluga>? result;
  bool isLoading = true;

  final TextEditingController _nazivController = TextEditingController();

  int _page = 1;
  static const int _pageSize = 10;

  @override
  void initState() {
    super.initState();
    _provider = context.read<UslugaProvider>();
    initTable();
  }

  Future<void> initTable() async {
    setState(() => isLoading = true);
    try {
      var data = await _provider.get(filter: {
        "naziv": _nazivController.text,
        "page": _page,
        "pageSize": _pageSize,
        "includeTotalCount": true,
      });
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
      title: "Usluge",
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            _buildSearch(),
            isLoading ? CircularProgressIndicator() : _buildTable(),
          ],
        ),
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
              DataColumn(label: Text("Naziv")),
              DataColumn(label: Text("Kategorija")),
              DataColumn(label: Text("Cijena")),
              DataColumn(label: Text("Trajanje (min)")),
              DataColumn(label: Text("Aktivna")),
              DataColumn(label: Text("Obriši")),
            ],
            rows: result?.items
                    ?.map(
                      (e) => DataRow(
                        onSelectChanged: (value) async {
                          var refresh = await Navigator.of(context).push(
                            MaterialPageRoute(
                              builder: (context) =>
                                  UslugaDetailsScreen(usluga: e),
                            ),
                          );
                          if (refresh == "reload") initTable();
                        },
                        cells: [
                          DataCell(Text(e.naziv ?? '')),
                          DataCell(Text(e.uslugaKategorijaNaziv ?? '')),
                          DataCell(Text(
                              e.cijena != null ? "${e.cijena} KM" : '')),
                          DataCell(Text(e.trajanjeMinuta?.toString() ?? '')),
                          DataCell(Text(e.isActive == true ? "Da" : "Ne")),
                          DataCell(
                            IconButton(
                              icon: Icon(Icons.delete),
                              onPressed: () async {
                                showDialog(
                                  context: context,
                                  builder: (context) => AlertDialog(
                                    title: Text("Brisanje"),
                                    content: Text(
                                        "Jeste li sigurni da želite obrisati ovu uslugu?"),
                                    actions: [
                                      TextButton(
                                        onPressed: () =>
                                            Navigator.pop(context),
                                        child: Text("Otkaži"),
                                      ),
                                      ElevatedButton(
                                        onPressed: () async {
                                          try {
                                            await _provider.remove(e.id!);
                                            Navigator.pop(context);
                                            setState(() {
                                              initTable();
                                            });
                                          } on Exception catch (e) {
                                            alertBoxMoveBack(
                                                context, "Greška", e.toString());
                                          }
                                        },
                                        child: Text("Da"),
                                      ),
                                    ],
                                  ),
                                );
                              },
                            ),
                          ),
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
                  ? "Nema usluga"
                  : "Prikazano $prikazanoOd do $prikazanoDo od ukupno $totalCount usluga"),
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

  Padding _buildSearch() {
    return Padding(
      padding: const EdgeInsets.all(8.0),
      child: Row(
        children: [
          Expanded(
            child: Padding(
              padding: const EdgeInsets.all(8.0),
              child: TextField(
                controller: _nazivController,
                decoration: InputDecoration(label: Text("Naziv")),
              ),
            ),
          ),
          ElevatedButton(
            onPressed: () {
              _page = 1;
              initTable();
            },
            child: Text("Pretraži"),
          ),
          SizedBox(width: 10),
          ElevatedButton(
            onPressed: () async {
              var refresh = await Navigator.of(context).push(
                MaterialPageRoute(
                  builder: (context) => const UslugaDetailsScreen(usluga: null),
                ),
              );
              if (refresh == "reload") initTable();
            },
            child: Text("Nova"),
          ),
        ],
      ),
    );
  }
}
