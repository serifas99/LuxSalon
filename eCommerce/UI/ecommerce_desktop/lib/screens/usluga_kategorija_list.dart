import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/models/search_result.dart';
import 'package:ecommerce_desktop/providers/usluga_kategorija_provider.dart';
import 'package:ecommerce_desktop/screens/usluga_kategorija_details_screen.dart';
import 'package:ecommerce_desktop/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/usluga_kategorija.dart';

class UslugaKategorijaList extends StatefulWidget {
  const UslugaKategorijaList({super.key});

  @override
  State<UslugaKategorijaList> createState() => _UslugaKategorijaListState();
}

class _UslugaKategorijaListState extends State<UslugaKategorijaList> {
  late UslugaKategorijaProvider _provider;
  SearchResult<UslugaKategorija>? result;
  bool isLoading = true;

  final TextEditingController _nazivController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _provider = context.read<UslugaKategorijaProvider>();
    initTable();
  }

  Future<void> initTable() async {
    try {
      var data = await _provider.get(filter: {});
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
      title: "Kategorije usluga",
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
    return Expanded(
      child: SizedBox(
        width: double.infinity,
        child: SingleChildScrollView(
          child: DataTable(
            columns: [
              DataColumn(label: Text("Naziv")),
              DataColumn(label: Text("Opis")),
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
                                  UslugaKategorijaDetailsScreen(
                                      kategorija: e),
                            ),
                          );
                          if (refresh == "reload") initTable();
                        },
                        cells: [
                          DataCell(Text(e.naziv ?? '')),
                          DataCell(Text(e.opis ?? '')),
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
                                        "Jeste li sigurni da želite obrisati ovu kategoriju?"),
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
            onPressed: () async {
              try {
                var data = await _provider
                    .get(filter: {"naziv": _nazivController.text});
                setState(() {
                  result = data;
                  isLoading = false;
                });
              } on Exception catch (e) {
                alertBox(context, 'Greška', e.toString());
              }
            },
            child: Text("Pretraži"),
          ),
          SizedBox(width: 10),
          ElevatedButton(
            onPressed: () async {
              var refresh = await Navigator.of(context).push(
                MaterialPageRoute(
                  builder: (context) =>
                      const UslugaKategorijaDetailsScreen(kategorija: null),
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
