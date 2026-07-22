import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/models/search_result.dart';
import 'package:ecommerce_desktop/providers/frizer_provider.dart';
import 'package:ecommerce_desktop/screens/frizer_details_screen.dart';
import 'package:ecommerce_desktop/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/frizer.dart';

class FrizerList extends StatefulWidget {
  const FrizerList({super.key});

  @override
  State<FrizerList> createState() => _FrizerListState();
}

class _FrizerListState extends State<FrizerList> {
  late FrizerProvider _provider;
  SearchResult<Frizer>? result;
  bool isLoading = true;

  final TextEditingController _imeController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _provider = context.read<FrizerProvider>();
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
      title: "Frizeri",
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
              DataColumn(label: Text("Ime i prezime")),
              DataColumn(label: Text("Email")),
              DataColumn(label: Text("Specijalizacija")),
              DataColumn(label: Text("Aktivan")),
              DataColumn(label: Text("Obriši")),
            ],
            rows: result?.items
                    ?.map(
                      (e) => DataRow(
                        onSelectChanged: (value) async {
                          var refresh = await Navigator.of(context).push(
                            MaterialPageRoute(
                              builder: (context) =>
                                  FrizerDetailsScreen(frizer: e),
                            ),
                          );
                          if (refresh == "reload") initTable();
                        },
                        cells: [
                          DataCell(Text(e.imePrezime ?? '')),
                          DataCell(Text(e.email ?? '')),
                          DataCell(Text(e.specijalizacija ?? '')),
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
                                        "Jeste li sigurni da želite obrisati ovog frizera?"),
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
                controller: _imeController,
                decoration: InputDecoration(label: Text("Ime i prezime")),
              ),
            ),
          ),
          ElevatedButton(
            onPressed: () async {
              try {
                var data = await _provider
                    .get(filter: {"imePrezime": _imeController.text});
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
                  builder: (context) => const FrizerDetailsScreen(frizer: null),
                ),
              );
              if (refresh == "reload") initTable();
            },
            child: Text("Novi"),
          ),
        ],
      ),
    );
  }
}
