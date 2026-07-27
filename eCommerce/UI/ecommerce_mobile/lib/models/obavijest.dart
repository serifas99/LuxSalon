import 'package:json_annotation/json_annotation.dart';

part 'obavijest.g.dart';

@JsonSerializable()
class Obavijest {
  final int? id;
  final String? naslov;
  final String? tekst;
  final String? slikaBase64;
  final bool? isActive;
  final DateTime? createdAt;

  Obavijest({
    this.id,
    this.naslov,
    this.tekst,
    this.slikaBase64,
    this.isActive,
    this.createdAt,
  });

  factory Obavijest.fromJson(Map<String, dynamic> json) =>
      _$ObavijestFromJson(json);

  Map<String, dynamic> toJson() => _$ObavijestToJson(this);
}
