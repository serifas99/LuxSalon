import 'package:ecommerce_mobile/models/search_result.dart';
import 'package:ecommerce_mobile/models/usluga.dart';
import 'package:ecommerce_mobile/models/usluga_kategorija.dart';
import 'package:ecommerce_mobile/models/usluga_preporuka.dart';
import 'package:ecommerce_mobile/providers/auth_provider.dart';
import 'package:ecommerce_mobile/providers/recommendation_provider.dart';
import 'package:ecommerce_mobile/providers/usluga_kategorija_provider.dart';
import 'package:ecommerce_mobile/providers/usluga_provider.dart';
import 'package:ecommerce_mobile/screens/usluga_details_screen.dart';
import 'package:ecommerce_mobile/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  late UslugaProvider _uslugaProvider;
  late UslugaKategorijaProvider _kategorijaProvider;
  late RecommendationProvider _recommendationProvider;

  SearchResult<Usluga>? _usluge;
  SearchResult<UslugaKategorija>? _kategorije;
  List<UslugaPreporuka> _preporuke = [];

  int? _odabranaKategorija;
  bool _isLoading = true;

  int get _klijentId =>
      int.tryParse(AuthProvider.accessTokenDecoded?['Id']?.toString() ?? '') ??
      0;

  @override
  void initState() {
    super.initState();
    _uslugaProvider = context.read<UslugaProvider>();
    _kategorijaProvider = context.read<UslugaKategorijaProvider>();
    _recommendationProvider = context.read<RecommendationProvider>();
    _ucitaj();
  }

  Future _ucitaj() async {
    try {
      final kategorije = await _kategorijaProvider.get(filter: {"pageSize": 1000});
      final usluge = await _uslugaProvider.get(filter: {
        "pageSize": 1000,
        "isActive": true,
        if (_odabranaKategorija != null)
          "uslugaKategorijaId": _odabranaKategorija,
      });

      List<UslugaPreporuka> preporuke = [];
      try {
        preporuke = await _recommendationProvider.preporuke(_klijentId, broj: 5);
      } catch (_) {
        // preporuke nisu kriticne za rad ekrana - ignorisi gresku
      }

      if (!mounted) return;
      setState(() {
        _kategorije = kategorije;
        _usluge = usluge;
        _preporuke = preporuke;
        _isLoading = false;
      });
    } on Exception catch (e) {
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }

  Future _filtrirajPoKategoriji(int? kategorijaId) async {
    setState(() {
      _odabranaKategorija = kategorijaId;
      _isLoading = true;
    });
    await _ucitaj();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("LuxSalon"),
        centerTitle: true,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _ucitaj,
              child: ListView(
                padding: const EdgeInsets.only(bottom: 24),
                children: [
                  if (_preporuke.isNotEmpty) _buildPreporuke(),
                  _buildKategorijeFilter(),
                  _buildUslugeGrid(),
                ],
              ),
            ),
    );
  }

  Widget _buildPreporuke() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Padding(
          padding: EdgeInsets.fromLTRB(16, 16, 16, 8),
          child: Text(
            "Preporučeno za vas",
            style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
          ),
        ),
        SizedBox(
          height: 170,
          child: ListView.builder(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 12),
            itemCount: _preporuke.length,
            itemBuilder: (context, index) {
              final p = _preporuke[index];
              final usluga = p.usluga;
              if (usluga == null) return const SizedBox.shrink();
              return GestureDetector(
                onTap: () => Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => UslugaDetailsScreen(usluga: usluga),
                  ),
                ),
                child: Container(
                  width: 220,
                  margin: const EdgeInsets.symmetric(horizontal: 4),
                  child: Card(
                    color: Colors.red.shade50,
                    elevation: 3,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                      side: BorderSide(color: Colors.red.shade200),
                    ),
                    child: Padding(
                      padding: const EdgeInsets.all(12),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: [
                              Icon(Icons.auto_awesome,
                                  size: 16, color: Colors.red.shade400),
                              const SizedBox(width: 4),
                              Text(
                                "${((p.skor ?? 0) * 100).toStringAsFixed(0)}% poklapanje",
                                style: TextStyle(
                                  fontSize: 11,
                                  color: Colors.red.shade400,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 6),
                          Text(
                            usluga.naziv ?? '',
                            style: const TextStyle(
                                fontWeight: FontWeight.bold, fontSize: 15),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                          const SizedBox(height: 4),
                          Text(
                            p.objasnjenje ?? '',
                            style: const TextStyle(fontSize: 12),
                            maxLines: 3,
                            overflow: TextOverflow.ellipsis,
                          ),
                          const Spacer(),
                          Text(
                            usluga.cijena != null
                                ? "${usluga.cijena} KM"
                                : '',
                            style: const TextStyle(fontWeight: FontWeight.bold),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              );
            },
          ),
        ),
      ],
    );
  }

  Widget _buildKategorijeFilter() {
    final kategorije = _kategorije?.items ?? [];
    return SizedBox(
      height: 44,
      child: ListView(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
        children: [
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 4),
            child: ChoiceChip(
              label: const Text("Sve"),
              selected: _odabranaKategorija == null,
              onSelected: (_) => _filtrirajPoKategoriji(null),
            ),
          ),
          ...kategorije.map(
            (k) => Padding(
              padding: const EdgeInsets.symmetric(horizontal: 4),
              child: ChoiceChip(
                label: Text(k.naziv ?? ''),
                selected: _odabranaKategorija == k.id,
                onSelected: (_) => _filtrirajPoKategoriji(k.id),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildUslugeGrid() {
    final usluge = _usluge?.items ?? [];
    if (usluge.isEmpty) {
      return const Padding(
        padding: EdgeInsets.all(32),
        child: Center(child: Text("Nema usluga u ovoj kategoriji")),
      );
    }
    return GridView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 2,
        childAspectRatio: 0.85,
        crossAxisSpacing: 8,
        mainAxisSpacing: 8,
      ),
      itemCount: usluge.length,
      itemBuilder: (context, index) {
        final u = usluge[index];
        return GestureDetector(
          onTap: () => Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => UslugaDetailsScreen(usluga: u),
            ),
          ),
          child: Card(
            elevation: 2,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(12),
            ),
            child: Padding(
              padding: const EdgeInsets.all(12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Icon(Icons.content_cut, color: Colors.red.shade300),
                  const SizedBox(height: 8),
                  Text(
                    u.naziv ?? '',
                    style: const TextStyle(fontWeight: FontWeight.bold),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    u.uslugaKategorijaNaziv ?? '',
                    style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
                  ),
                  const Spacer(),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text(
                        u.cijena != null ? "${u.cijena} KM" : '',
                        style: const TextStyle(fontWeight: FontWeight.bold),
                      ),
                      Text(
                        u.trajanjeMinuta != null ? "${u.trajanjeMinuta} min" : '',
                        style: TextStyle(fontSize: 11, color: Colors.grey.shade600),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
        );
      },
    );
  }
}
