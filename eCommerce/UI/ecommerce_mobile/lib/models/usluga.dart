import 'package:json_annotation/json_annotation.dart';

part 'usluga.g.dart';

@JsonSerializable()
class Usluga {
  final int? id;
  final String? naziv;
  final String? opis;
  final double? cijena;
  final int? trajanjeMinuta;
  final int? uslugaKategorijaId;
  final String? uslugaKategorijaNaziv;
  final String? tagovi;
  final bool? isActive;
  final DateTime? createdAt;
  final DateTime? updatedAt;

  Usluga({
    this.id,
    this.naziv,
    this.opis,
    this.cijena,
    this.trajanjeMinuta,
    this.uslugaKategorijaId,
    this.uslugaKategorijaNaziv,
    this.tagovi,
    this.isActive,
    this.createdAt,
    this.updatedAt,
  });

  factory Usluga.fromJson(Map<String, dynamic> json) =>
      _$UslugaFromJson(json);

  Map<String, dynamic> toJson() => _$UslugaToJson(this);
}
